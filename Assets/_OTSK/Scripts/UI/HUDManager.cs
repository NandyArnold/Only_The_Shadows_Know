using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Required for Coroutines

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; } // Make it a singleton

    [Header("UI Panels")]
    
    [SerializeField] private GameObject crosshairPanel;

    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
  

    [Header("Objective UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private float objectiveDisplayTime = 4f;
    [SerializeField] private float objectiveFadeTime = 0.5f;

    [SerializeField] private GameObject crosshairPrefab;

    [Header("Debug UI")]
    [SerializeField] private TextMeshProUGUI aimingDebugText;
    [SerializeField] private TextMeshProUGUI focusedDebugText;

    private CursorState _currentCursorState;

    private Coroutine _objectiveCoroutine;
    private PlayerCombat _playerCombatForDebug;
    private GameObject _crosshairInstance;

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

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.alpha = 0; // Start fully transparent
        }
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (manaSlider != null) manaSlider.gameObject.SetActive(false);
      
        if (crosshairPanel != null) crosshairPanel.SetActive(false);
       
    }

    private void Update() 
    { 
        if (_playerCombatForDebug != null) 
        { 
            aimingDebugText.text = $"isAiming: {_playerCombatForDebug.IsAiming}"; focusedDebugText.text = $"isFocused: {_playerCombatForDebug.IsFocused}";
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

        if (crosshairPrefab != null && crosshairPanel != null && _crosshairInstance == null)
        {
            _crosshairInstance = Instantiate(crosshairPrefab, crosshairPanel.transform);
        }
    }


    private void HandleNewObjective(ObjectiveSO newObjective)
    {
        if (objectiveText == null) return;

        if (_objectiveCoroutine != null)
        {
            StopCoroutine(_objectiveCoroutine);
        }

        if (newObjective != null)
        {
            _objectiveCoroutine = StartCoroutine(FadeObjectiveText(newObjective.objectiveDescription));
        }
    }

    private IEnumerator FadeObjectiveText(string text)
    {
        // Fade Out (if it was already visible)
        float startAlpha = objectiveText.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            objectiveText.alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set text and Fade In
        objectiveText.text = text;
        elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            objectiveText.alpha = Mathf.Lerp(0, 1, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectiveText.alpha = 1;

        // Wait for a few seconds
        yield return new WaitForSeconds(objectiveDisplayTime);

        // Fade Out
        elapsedTime = 0f;
        while (elapsedTime < objectiveFadeTime)
        {
            objectiveText.alpha = Mathf.Lerp(1, 0, elapsedTime / objectiveFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectiveText.alpha = 0;
    }

    private void HandleLevelCompleted()
    {
        if (_objectiveCoroutine != null)
        {
            StopCoroutine(_objectiveCoroutine);
        }
        _objectiveCoroutine = StartCoroutine(FadeObjectiveText("All Objectives Complete!"));
    }

    // --- (The rest of your HUDManager script is unchanged) ---

    private void HandleShowObjective()
    {
        // Get the current objective from the manager
        ObjectiveSO currentObjective = ObjectiveManager.Instance.GetCurrentObjective();
        if (currentObjective != null)
        {
            // Re-run the same fade coroutine we already have
            if (_objectiveCoroutine != null) StopCoroutine(_objectiveCoroutine);
            _objectiveCoroutine = StartCoroutine(FadeObjectiveText(currentObjective.objectiveDescription));
        }
    }
    public void RegisterPlayerForDebugging(PlayerCombat playerCombat) { _playerCombatForDebug = playerCombat; }

    private void HandleGameStateChanged(GameState newState)
    {
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

  

}