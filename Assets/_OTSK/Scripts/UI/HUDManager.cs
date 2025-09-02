using System.Collections; // Required for Coroutines
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Ensure you have DOTween installed for the fade effects

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; } // Make it a singleton

    [Header("UI Panels")]
    [SerializeField] private GameObject playerHudPanel;
   

    [SerializeField] private GameObject crosshairPanel;
    [Header("Weapon UI")] 
    [SerializeField] private Image weaponIconImage;

    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
  

    [Header("Objective UI")]
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private float objectiveDisplayTime = 4f;
    [SerializeField] private float objectiveFadeTime = 0.5f;

    [SerializeField] private GameObject crosshairPrefab;

    [Header("Scrying UI")]
    [SerializeField] private GameObject minimapPanel;
    [SerializeField] private UnityEngine.UI.RawImage minimapRenderImage;
    [SerializeField] private RectTransform objectiveArrow;
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

    private CanvasGroup _objectivePanelCG;
    private Coroutine _objectiveCoroutine;

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

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnStateChanged += HandleCursorStateChanged;
        }

        if (playerHudPanel != null)
        {
            playerHudPanel.SetActive(false); // Initially hide the player HUD
        }

        if (objectivePanel != null)
        {
            _objectivePanelCG = objectivePanel.GetComponent<CanvasGroup>();
            if (_objectivePanelCG != null) _objectivePanelCG.alpha = 0;
        }


        
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (manaSlider != null) manaSlider.gameObject.SetActive(false);
      
        if (crosshairPanel != null) crosshairPanel.SetActive(false);

        if (minimapPanel != null) minimapPanel.SetActive(false);
        if (objectiveArrow != null) objectiveArrow.gameObject.SetActive(false);


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

        if (minimapPanel.activeSelf)
        {
            UpdateObjectiveArrow();
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
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged -= HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
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

        if (player.TryGetComponent<PlayerInputHandler>(out var inputHandler))
        {
            inputHandler.OnShowObjectiveInput += HandleShowObjective;
        }

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


    private void HandleNewObjective(ObjectiveSO newObjective)
    {
        if (objectiveText == null) return;

        if (newObjective != null)
        {
            // A valid objective is active, so we store its location for the arrow.
            _currentObjectiveTransform = newObjective.objectiveLocation;
        }
        else
        {
            // No active objective (e.g., in Main Menu or between levels), so we clear the target.
            // This stops the arrow from pointing to an old objective.
            _currentObjectiveTransform = null;
        }

        if (gameObject.activeInHierarchy)
        {
            if (_objectiveCoroutine != null)
            {
                StopCoroutine(_objectiveCoroutine);
            }

            if (newObjective != null)
            {
                _objectiveCoroutine = StartCoroutine(ShowObjectiveCoroutine(newObjective.objectiveDescription));
            }
        }
    }

    private IEnumerator ShowObjectiveCoroutine(string text)
    {
        UISoundPlayer.Instance.PlayNewObjectiveSound();
        // Fade Out (if it was already visible)
        float startAlpha = _objectivePanelCG.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            _objectivePanelCG.alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set text and Fade In
        objectiveText.text = text;
        elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            _objectivePanelCG.alpha = Mathf.Lerp(0, 1, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _objectivePanelCG.alpha = 1;

        // Wait for the display time
        yield return new WaitForSecondsRealtime(objectiveDisplayTime);

        // Fade Out again
        elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            _objectivePanelCG.alpha = Mathf.Lerp(1, 0, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _objectivePanelCG.alpha = 0;
    }
    

    private void HandleLevelCompleted()
    {
        if (_objectiveCoroutine != null)
        {
            StopCoroutine(_objectiveCoroutine);
        }
        _objectiveCoroutine = StartCoroutine(ShowObjectiveCoroutine("All Objectives Complete!"));
    }

    // --- (The rest of your HUDManager script is unchanged) ---

    private void HandleShowObjective()
    {
        // Get the current objective from the manager
        ObjectiveSO currentObjective = ObjectiveManager.Instance.GetCurrentObjective();
        if (currentObjective != null)
        {
            UISoundPlayer.Instance.PlayToggleSound();
            // Re-run the same fade coroutine we already have
            if (_objectiveCoroutine != null) StopCoroutine(_objectiveCoroutine);
            _objectiveCoroutine = StartCoroutine(ShowObjectiveCoroutine(currentObjective.objectiveDescription));
        }
    }
    public void RegisterPlayerForDebugging(PlayerCombat playerCombat) { _playerCombatForDebug = playerCombat; }

    private void HandleGameStateChanged(GameState newState)
    {
        if(objectivePanel != null)
        {
            // Show the objective panel only during Gameplay state
            objectivePanel.SetActive(newState == GameState.Gameplay);
        }
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
        objectiveArrow.gameObject.SetActive(true);
    }

    public void HideMinimap()
    {
        minimapPanel.SetActive(false);
        objectiveArrow.gameObject.SetActive(false);
    }

    private void UpdateObjectiveArrow()
    {
        // Fail-safe: If either the arrow, the objective, or the player is missing, hide the arrow and stop.
        if (objectiveArrow == null || _currentObjectiveTransform == null || _playerController == null)
        {
            if (objectiveArrow != null && objectiveArrow.gameObject.activeSelf)
            {
                objectiveArrow.gameObject.SetActive(false);
            }
            return;
        }

        // Convert objective's world position to a screen position relative to the main camera
        Vector3 objectiveScreenPos = Camera.main.WorldToScreenPoint(_currentObjectiveTransform.position);

        RectTransform minimapRect = minimapPanel.GetComponent<RectTransform>();

        // Check if the objective is visible on the minimap
        if (RectTransformUtility.RectangleContainsScreenPoint(minimapRect, objectiveScreenPos, Camera.main))
        {
            objectiveArrow.gameObject.SetActive(false);
        }
        else
        {
            objectiveArrow.gameObject.SetActive(true);

            // Calculate the direction from the player to the objective
            Vector3 direction = (_currentObjectiveTransform.position - _playerController.transform.position);

            // We only care about the direction on the horizontal plane (X and Z axes)
            direction.y = 0;

            // Calculate the angle from the forward direction of the player
            float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);

            // Apply the rotation. The offset (-90, etc.) depends on the original orientation of your arrow sprite.
            // If your arrow image points "Up", you need no offset. If it points "Right", you need a -90 degree offset.
            objectiveArrow.localEulerAngles = new Vector3(0, 0, -angle);
        }
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