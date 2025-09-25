using System.Collections; 
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; } 

    [Header("UI Panels")]
    [SerializeField] private GameObject playerHudPanel;
   

    [SerializeField] private GameObject crosshairPanel;
    [Header("Weapon UI")] 
    [SerializeField] private Image weaponIconImage;

    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
  


    [SerializeField] private GameObject crosshairPrefab;

    [Header("Scrying UI")]
    [SerializeField] private GameObject minimapPanel;
    [SerializeField] private UnityEngine.UI.RawImage minimapRenderImage;
   
    private Transform _currentObjectiveTransform;

    [Header("Charge/Ammo UI")]
    [SerializeField] private GameObject chargePanel;
    [SerializeField] private TextMeshProUGUI chargeCountText;

    [Header("Debug UI")]
    [SerializeField] private TextMeshProUGUI aimingDebugText;
    [SerializeField] private TextMeshProUGUI focusedDebugText;
    [SerializeField] private TextMeshProUGUI invulnerabilityText;

    private CursorState _currentCursorState;
    private GameObject _crosshairInstance;



    private PlayerCombat _playerCombatForDebug;
    private Invulnerability _invulnerability;
    private ChargeManager _chargeManager;
    private PlayerController _playerController;




    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
        }


        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnStateChanged += HandleCursorStateChanged;
        }

        if (playerHudPanel != null)
        {
            playerHudPanel.SetActive(false); // Initially hide the player HUD
        }
   
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (manaSlider != null) manaSlider.gameObject.SetActive(false);
      
        if (crosshairPanel != null) crosshairPanel.SetActive(false);

        if (minimapPanel != null) minimapPanel.SetActive(false);

    }

    private void Update() 
    { 
        if (_playerCombatForDebug != null) 
        { 
            aimingDebugText.text = $"isAiming: {_playerCombatForDebug.IsAiming}";
            focusedDebugText.text = $"isFocused: {_playerCombatForDebug.IsFocused}";
        }

        if (_invulnerability != null)
        {
            
            // This is the correct call
            invulnerabilityText.text = $"Invulnerable: {_invulnerability.IsInvulnerable}";
        }
        else
        {
            invulnerabilityText.text = "Invulnerable: N/A";
        }

    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnStateChanged -= HandleCursorStateChanged;
        }
    }

    private void HandlePlayerRegistered(PlayerController player)
    {
        _playerController = player;
        _invulnerability = player.GetComponent<Invulnerability>();
        // Now that we have a valid player, we can get its components and subscribe to events.
        StartCoroutine(InitializePlayerHUDCoroutine(player));
    }

    private IEnumerator InitializePlayerHUDCoroutine(PlayerController player)
    {
        // Wait for the end of the frame to ensure all Awake() and Start() on the player have run.
        yield return new WaitForEndOfFrame();

        RegisterPlayerForDebugging(player.GetComponent<PlayerCombat>());

        if (player.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.OnHealthChanged += UpdateHealthBar;
            stats.OnManaChanged += UpdateManaBar;
           

            // Now we can safely get the initial values.
            UpdateHealthBar(stats.CurrentHealth, 100f);
            UpdateManaBar(stats.CurrentMana, 100f);
            

            healthSlider.gameObject.SetActive(true);
            manaSlider.gameObject.SetActive(true);
         
        }

        // 1. Get the ChargeManager and subscribe to its events
        if (player.TryGetComponent<ChargeManager>(out _chargeManager))
        {
            _chargeManager.OnChargeCountChanged += UpdateChargeCount;
        }

        // 2. Also listen for weapon switches to update the display
        if (player.TryGetComponent<PlayerCombat>(out var combat))
        {
            combat.OnWeaponSwitched += HandleWeaponSwitched;
        }

        HandleWeaponSwitched(combat.CurrentWeapon);


        if (crosshairPrefab != null && crosshairPanel != null && _crosshairInstance == null)
        {
            _crosshairInstance = Instantiate(crosshairPrefab, crosshairPanel.transform);
        }
    }


    
    public void RegisterPlayerForDebugging(PlayerCombat playerCombat) { _playerCombatForDebug = playerCombat; }

    private void HandleGameStateChanged(GameState newState)
    {
        
        bool shouldBeActive = (newState == GameState.Gameplay ||  newState == GameState.Details);

        if (playerHudPanel != null)
        {
            playerHudPanel.SetActive(shouldBeActive);
        }

        if (crosshairPanel != null)
        {
            // Only show the gameplay HUD when the game is in the Gameplay state.
            crosshairPanel.SetActive(newState == GameState.Gameplay);
        }
       
    }

    private void HandleCursorStateChanged(CursorState newState)
    {
        if (_crosshairInstance != null)
        {
            // The crosshair should only be visible when the cursor state is Gameplay.
            // It will be hidden during UI and Targeting.
            _crosshairInstance.SetActive(newState == CursorState.Gameplay);
        }
    }



    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null) healthSlider.value = currentHealth / maxHealth;
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaSlider != null) manaSlider.value = currentMana / maxMana;
    }

    private void HandleWeaponSwitched(WeaponSO newWeapon)
    {
        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = newWeapon.weaponIcon;
        }
        if (newWeapon is BowSO bow && bow.ammoType != null)
        {
            chargePanel.SetActive(true);
            int currentCharges = _chargeManager.GetChargeCount(bow.ammoType);
            //Debug.Log($"<color=lightblue>[HUDManager]</color> HandleWeaponSwitched: Bow equipped. Getting charge count. Value is: {currentCharges}. Updating UI.");
            UpdateChargeCount(bow.ammoType, _chargeManager.GetChargeCount(bow.ammoType));
        }
        else
        {
            //Debug.Log("<color=lightblue>[HUDManager]</color> HandleWeaponSwitched: Non-bow weapon equipped. Hiding charge panel.");
            // Hide the ammo count for weapons that don't use it
            chargePanel.SetActive(false);
        }
    }

    private void UpdateChargeCount(ChargeableItemSO item, int count)
    {
        // This method now only cares about the item it was told about.
        // We can make it smarter to handle multiple ammo types later if needed.
        if (item.name == "Chargeable_Arrows") // A simple check to make sure we're updating the right text
        {
            //Debug.Log($"<color=lightblue>[HUDManager]</color> UpdateChargeCount event received. Setting charge text to: {count}.");
            chargeCountText.text = $"Arrows: {count}";
        }
    }

    public void ShowMinimap(RenderTexture texture)
    {
        minimapRenderImage.texture = texture;
        minimapPanel.SetActive(true);
        
    }

    public void HideMinimap()
    {
        minimapPanel.SetActive(false);
     
    }

    

    public void SetGameplayHUDVisibility(bool isVisible)
    {
        // This method will now control both panels.
        if (playerHudPanel != null)
        {
            playerHudPanel.SetActive(isVisible);
        }

        // Also toggle the minimap panel's visibility.
        if (minimapPanel != null)
        {
            // Exception: Don't show the minimap if the eye isn't deployed yet.
            if (isVisible && ScryingSystem.Instance != null && !ScryingSystem.Instance.IsScryingDeployed)
            {
                minimapPanel.SetActive(false);
            }
            else
            {
                minimapPanel.SetActive(isVisible);
            }
        }
    }
}