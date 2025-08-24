// EnemyUIController.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EnemyUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider alertSlider;
    [SerializeField] private GameObject deathZoneMark;
    [SerializeField] private Slider castBarSlider;
    private GameObject _revealIconInstance;
    private EnemyConfigSO _config;
    private EnemyAI _enemyAI;
    private DetectionSystem _detector;
    private EnemyHealth _health;
    [Header("VFX")]
    [Tooltip("Drag the child Alert_VFX GameObject here.")]
    [SerializeField] private GameObject alertVFXObject;

    private Enemy _enemy;
    private Transform _cameraToFace;
    private EnemyAIState _currentState;


    private void OnDestroy()
    {
        // When this UI is destroyed, it must clean up its own event subscriptions
        // to prevent memory leaks and broken connections.
        if (_detector != null) _detector.OnSoundGaugeChanged -= UpdateAlert;
        if (_enemyAI != null)
        {
            _enemyAI.OnStateChanged -= HandleAIStateChanged;
            _enemyAI.OnCastProgressChanged -= UpdateCastBar;
        }
        if (_health != null) _health.OnHealthChanged -= UpdateHealth;
    }


    private void LateUpdate()
    {
        if (_cameraToFace == null)
        {
            if (Camera.main != null)
            {
                _cameraToFace = Camera.main.transform;
            }
            else
            {
                // If there is no main camera in the scene yet, we can't do anything.
                return;
            }
        }
        // Make the canvas billboard towards the camera
        transform.LookAt(transform.position + _cameraToFace.rotation * Vector3.forward, _cameraToFace.rotation * Vector3.up);
    }

    private void Start() // Use start instead of awake to ensure parent is ready
    {
        _enemy = GetComponentInParent<Enemy>();
    }

    public void UpdateHealth(float current, float max)
    {
        if (current <= 0)
        {
            Destroy(gameObject); // 'gameObject' refers to the object this script is on
            return; // Stop the rest of the method from running
        }
        healthSlider.gameObject.SetActive(current < max && current > 0); // Only show if not at full health
        healthSlider.value = current / max;
    }

    public void UpdateAlert(float current, float max)
    {

        if (_config == null) return;

        // --- THIS IS THE ROBUST LOGIC ---
        // The alert slider should be visible if:
        // 1. The alert value is greater than zero.
        // 2. We are NOT in CombatState.
        // 3. We are NOT in DeathState.
        bool inCombat = _currentState is CombatState;
        bool isDead = _currentState is DeathState;
        bool hasAlert = current > 0;

        bool shouldBeVisible = hasAlert && !inCombat && !isDead;

        if (alertSlider != null)
        {
            alertSlider.gameObject.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                alertSlider.value = current / max;
            }
        }
    }
    public void UpdateCastBar(float progress)
    {
        // Show the bar if progress is > 0, hide it otherwise
        castBarSlider.gameObject.SetActive(progress > 0);
        castBarSlider.value = progress;
    }

    public void HandleAIStateChanged(EnemyAIState newState)
    {
        _currentState = newState;

        if (newState is DeathState)
        {
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
            if (alertSlider != null) alertSlider.gameObject.SetActive(false);
            if (castBarSlider != null) castBarSlider.gameObject.SetActive(false);
            if (alertVFXObject != null) alertVFXObject.SetActive(false);
            Destroy(gameObject, 0.5f); // Optionally, destroy the status bar after 2 seconds
            return;
        }

        // The health bar should be visible in Alert and Combat states.
        bool showHealth = (newState is AlertState || newState is CombatState);
        healthSlider.gameObject.SetActive(showHealth);

        if (alertSlider != null && _detector != null) 
        {
            // We need the current gauge value. We can get it from the DetectionSystem.
            // This requires a small change to DetectionSystem.cs
            UpdateAlert(GetComponentInParent<Enemy>().Detector.SoundGauge, _config.hearingThreshold);
        }



        bool shouldShowAlertVFX = (newState is AlertState);
        if (alertVFXObject != null)
        {
            alertVFXObject.SetActive(shouldShowAlertVFX);
        }
    }
    public void SetDeathZoneMarkActive(bool isActive)
    {
        if (deathZoneMark != null)
        {
            deathZoneMark.SetActive(isActive);
        }
    }
    public void Initialize(EnemyAI ai, DetectionSystem detector, EnemyHealth health, EnemyConfigSO enemyConfig)
    {
        _enemyAI = ai; // Store the reference to the AI
        _config = enemyConfig;
        // Subscribe to all necessary events right here.
        detector.OnSoundGaugeChanged += UpdateAlert;
        ai.OnStateChanged += HandleAIStateChanged;
        health.OnHealthChanged += UpdateHealth; // Get health from the parameter
        ai.OnCastProgressChanged += UpdateCastBar;

        // Initialize the reveal icon from the AI's config.
        if (ai.Config.revealIconPrefab != null)
        {
            Transform anchor = transform.Find("RevealIcon_Anchor");
            if (anchor != null && _revealIconInstance == null)

            {
                _revealIconInstance = Instantiate(ai.Config.revealIconPrefab, anchor);
                _revealIconInstance.SetActive(false);
            }
        }

        // Set the initial state of all UI elements to hidden.
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        if (alertSlider != null) alertSlider.gameObject.SetActive(false);
        if (castBarSlider != null) castBarSlider.gameObject.SetActive(false);
    }

    //  This is the simple show/hide command Balor's Vision will use.
    public void SetRevealIconActive(bool isActive)
    {
        if (_revealIconInstance != null)
        {
            _revealIconInstance.SetActive(isActive);
        }
    }


}