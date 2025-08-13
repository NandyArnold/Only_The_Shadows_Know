// Enemy.cs - UPGRADED to handle death
using System.Collections;

using UnityEngine;
using UnityEngine.AI;


// We add [RequireComponent] to ensure a UniqueID is always present.
[RequireComponent(typeof(UniqueID))]
public class Enemy : MonoBehaviour, ISaveable
{

    // --- Save Data Structure ---
    [System.Serializable]
    private struct EnemySaveData
    {
        // For Position
        public float posX, posY, posZ;
        // For Rotation
        public float rotX, rotY, rotZ, rotW;
        // For Health
        public float currentHealth;
    }
    // --- ISaveable Implementation ---
    public string UniqueID => _uniqueID.ID;

    // --- Component References 
    [Header("Data")] [Tooltip("The enemy's configuration data, including stats and behavior.")]
    [SerializeField] private EnemyConfigSO config;

    [Header("UI")] // NEW
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private Transform statusBarAnchor;
    public EnemyConfigSO Config => config;
    public DetectionSystem Detector { get; private set; }

    // Component references
    private EnemyHealth _health;
    private EnemyNavigator _navigator;
    private EnemyAI _ai;
    private EnemyAnimationController _animController;
    private CapsuleCollider _collider;
    private UniqueID _uniqueID;
    private EnemyUIController _uiController;
    private GameObject _statusBarInstance;
    private PatrolRoute _patrolRoute;



    public object CaptureState()
    {
        return new EnemySaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,

            rotX = transform.rotation.x,
            rotY = transform.rotation.y,
            rotZ = transform.rotation.z,
            rotW = transform.rotation.w,

            currentHealth = _health.CurrentHealth
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (EnemySaveData)state;

        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
        transform.rotation = new Quaternion(saveData.rotX, saveData.rotY, saveData.rotZ, saveData.rotW);

        _health.SetHealth(saveData.currentHealth);
    }
    private void Awake()
    {
        Detector = GetComponent<DetectionSystem>();
        _health = GetComponent<EnemyHealth>();
        _navigator = GetComponent<EnemyNavigator>();
        _ai = GetComponent<EnemyAI>();
        _animController = GetComponent<EnemyAnimationController>();
        _collider = GetComponent<CapsuleCollider>();
        _uniqueID = GetComponent<UniqueID>();
        _patrolRoute = GetComponent<PatrolRoute>();

        statusBarAnchor = transform.Find("StatusBarAnchor");
        if (statusBarAnchor == null)
        {
            Debug.LogError("StatusBarAnchor child object not found on Enemy!", this.gameObject);
        }
    }

    private void OnEnable()
    {
        
        _health.OnDied += HandleDeath; // Subscribe to the death event
    }

    private void OnDisable()
    {
        
        _health.OnDied -= HandleDeath; // Unsubscribe
    }

    private void OnDestroy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }

        // ADD THIS: Notify the combat manager that this enemy is gone.
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterEnemyFromCombat(this);
        }
        if (_uiController != null)
        {
            _health.OnHealthChanged -= _uiController.UpdateHealth;
            Detector.OnSoundGaugeChanged -= _uiController.UpdateAlert;
            if (_ai != null)
            {
                _ai.OnStateChanged -= _uiController.HandleAIStateChanged;
            }
        }
    }

    public void Initialize(EnemyConfigSO newConfig, PatrolRoute newPatrolRoute, InitialAIState initialState)
    {
        //Debug.Log($"[Enemy] Initializing Enemy: {gameObject.name} with config: {newConfig.name}");
        this.config = newConfig;

        // Pass the config data down to all the sub-components.
        //Debug.Log($"[Enemy] Initializing _health components with config: {newConfig.name}");
        _health.Initialize(newConfig);
        //Debug.Log($"[Enemy] Initializing Detector components with config: {newConfig.name}");
        Detector.Initialize(newConfig);
        //Debug.Log($"[Enemy] Initializing EnemyAI components with config: {newConfig.name}");
        _ai.Config = newConfig;
        _ai.PatrolRoute = newPatrolRoute;
        _ai.InitialState = initialState;

        _ai.StartAI();
      

    }
    private void Start()
    {
        //Debug.Log($"[Enemy] Registering enemy: {gameObject.name} with ID: {_uniqueID.ID}");
        EnemyManager.Instance.RegisterEnemy(this);

        //_navigator.SetSpeed(config.patrolSpeed);

        //Instantiate and set up the status bar
        if (statusBarPrefab != null)
        {
            _statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, transform);
            _uiController = _statusBarInstance.GetComponent<EnemyUIController>();

            // Connect the standard events
            _health.OnHealthChanged += _uiController.UpdateHealth;
            Detector.OnSoundGaugeChanged += _uiController.UpdateAlert;

            if (_ai != null)
            {
                _ai.OnStateChanged += _uiController.HandleAIStateChanged;
                _uiController.HandleAIStateChanged(_ai.CurrentState);
            }

            // Initialize the health bar and alert bar to zero 
            _uiController.UpdateHealth(_health.CurrentHealth, config.maxHealth);
            _uiController.UpdateAlert(0, config.hearingThreshold);
        }
    
    }

    private void HandleDeath()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UnregisterEnemyFromCombat(this);
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
        //  Destroy the status bar immediately.
        if (_statusBarInstance != null)
        {
            Destroy(_statusBarInstance);
        }
        // 1. Play the death animation immediately.
        _animController.PlayDeathAnimation();
        // 1. Disable all intelligence and movement
        _ai.enabled = false;
        _navigator.Stop();
        //GetComponent<NavMeshAgent>().enabled = false;
        _navigator.enabled = false;
        //_collider.enabled = false;

        // 2. Play the death animation
        _animController.PlayDeathAnimation();

        gameObject.tag = "DeadBody";
        // 3. Start a timer to remove the body
        //StartCoroutine(CleanupBody(5f)); // Wait 5 seconds before removing body
    }

    private IEnumerator CleanupBody(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void HandlePlayerDeath()
    {
        // Disable senses
        //if (Detector != null) Detector.enabled = false;

        // Force AI to a safe state
        //if (_ai != null) _ai.ForceReturnToPatrol();
    }

    // NEW: This is also called by the EnemyManager
    public void HandlePlayerRespawn()
    {
        // Re-enable senses
        //if (Detector != null) Detector.enabled = true;
    }
}