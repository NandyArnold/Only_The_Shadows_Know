using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Required for Coroutines

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; } // Make it a singleton



    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider noiseSlider;

    [Header("Objective UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private float objectiveDisplayTime = 4f;
    [SerializeField] private float objectiveFadeTime = 0.5f;

    [Header("Debug UI")]
    [SerializeField] private TextMeshProUGUI aimingDebugText;
    [SerializeField] private TextMeshProUGUI focusedDebugText;



    private Coroutine _objectiveCoroutine;
    private PlayerCombat _playerCombatForDebug;

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

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.alpha = 0; // Start fully transparent
        }
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (manaSlider != null) manaSlider.gameObject.SetActive(false);
        if (noiseSlider != null) noiseSlider.gameObject.SetActive(false);
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
    }

    private void HandlePlayerRegistered(PlayerController player)
    {
        // Now that we have a valid player, we can get its components and subscribe to events.
        if (player.TryGetComponent<PlayerInputHandler>(out var inputHandler))
        {
            inputHandler.OnShowObjectiveInput += HandleShowObjective;
        }

        RegisterPlayerForDebugging(player.GetComponent<PlayerCombat>());

        // --- THIS IS THE KEY LOGIC ---
        // Get the stats component and subscribe to its events.
        if (player.TryGetComponent<PlayerHealthManaNoise>(out var stats))
        {
            stats.OnHealthChanged += UpdateHealthBar;
            stats.OnManaChanged += UpdateManaBar;
            stats.OnNoiseChanged += UpdateNoiseBar;

            // Set the initial values and make the bars visible
            UpdateHealthBar(stats.CurrentHealth, 100f); // Assuming max health is 100
            UpdateManaBar(stats.CurrentMana, 100f);   // Assuming max mana is 100
            UpdateNoiseBar(stats.CurrentNoise, 100f); // Assuming max noise is 100

            healthSlider.gameObject.SetActive(true);
            manaSlider.gameObject.SetActive(true);
            noiseSlider.gameObject.SetActive(true);
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

    private void HandleGameStateChanged(GameState newState) { /* ... */ }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null) healthSlider.value = currentHealth / maxHealth;
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaSlider != null) manaSlider.value = currentMana / maxMana;
    }

    private void UpdateNoiseBar(float currentNoise, float maxNoise)
    {
        if (noiseSlider != null) noiseSlider.value = currentNoise / maxNoise;
    }

}