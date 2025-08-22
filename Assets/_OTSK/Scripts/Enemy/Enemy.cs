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
    public struct EnemySaveData
    {
        public string configSOName;
        public string patrolRouteID;
        public InitialAIState initialState;

        // Transform
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;

        // Health
        public float currentHealth;

        // AI State
        public int summonCount;

        // Active State
        public bool wasActive;

        public int lastWaypointIndex;
    }
    // --- ISaveable Implementation ---
    public string UniqueID => _uniqueID.ID;

    private bool _isLoadedDead = false;

    // --- Component References 
    [Header("Data")] [Tooltip("The enemy's configuration data, including stats and behavior.")]
    [SerializeField] private EnemyConfigSO config;

    [Header("UI")] 
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private Transform statusBarAnchor;
    [SerializeField] private Transform revealIconAnchor;
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

    private EnemyResistances _resistances; 
    private RevealableEntity _revealableEntity;



    public object CaptureState()
    {
        int waypointIndex = -1; // Default to -1 (not patrolling)
        if (_ai.CurrentState is PatrolState patrolState)
        {
            waypointIndex = patrolState.GetCurrentWaypointIndex();
        }
        string routeID = (_ai.PatrolRoute != null) ? _ai.PatrolRoute.routeID : null;
        


        return new EnemySaveData
        {
            configSOName = config.name, // Save the name of the config SO

            patrolRouteID = routeID,
            initialState = _ai.InitialState,
            lastWaypointIndex = waypointIndex,

            // Transform Data
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            rotX = transform.rotation.x,
            rotY = transform.rotation.y,
            rotZ = transform.rotation.z,
            rotW = transform.rotation.w,

            // Health Data
            currentHealth = _health.CurrentHealth,

            // AI Data
            summonCount = _ai.SummonCount,

            // Active State Data
            wasActive = gameObject.activeSelf

        };
    }

    public void RestoreState(object state)
    {
       

        // --- THIS IS THE FIX ---
        // First, check if the enemy was dead in the save file.
        var saveData = (EnemySaveData)state;

        if (!saveData.wasActive || saveData.currentHealth <= 0)
        {
            _isLoadedDead = true; // Set the flag
            return; 
        }

      
        _ai.InitialState = saveData.initialState;

        if (!string.IsNullOrEmpty(saveData.patrolRouteID))
        {
            PatrolRoute route = PatrolRouteManager.Instance.GetRoute(saveData.patrolRouteID);
            if (route != null)
            {
                _ai.PatrolRoute = route; // Assign the route to the AI
            }
            else
            {
                Debug.LogWarning($"Could not find Patrol Route with ID '{saveData.patrolRouteID}' for enemy {UniqueID}.");
            }
        }

        // If the enemy was alive, restore its full state.

        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
        transform.rotation = new Quaternion(saveData.rotX, saveData.rotY, saveData.rotZ, saveData.rotW);

        _health.SetHealth(saveData.currentHealth);
        _ai.SetSummonCount(saveData.summonCount); // You'll need to add this method to EnemyAI

        if (_statusBarInstance == null && saveData.currentHealth > 0)
        {
            _statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, transform);
            _uiController = _statusBarInstance.GetComponent<EnemyUIController>();

            // Re-connect the standard events
            _health.OnHealthChanged += _uiController.UpdateHealth;
            Detector.OnSoundGaugeChanged += _uiController.UpdateAlert;
            if (_ai != null)
            {
                _ai.OnStateChanged += _uiController.HandleAIStateChanged;
            }
        }

        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
        transform.rotation = new Quaternion(saveData.rotX, saveData.rotY, saveData.rotZ, saveData.rotW);

        _health.SetHealth(saveData.currentHealth);
        //_ai.StartAI(saveData.lastWaypointIndex);
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
        _resistances = GetComponent<EnemyResistances>(); 
        _revealableEntity = GetComponent<RevealableEntity>();
        revealIconAnchor = transform.Find("RevealIcon_Anchor");

        statusBarAnchor = transform.Find("StatusBarAnchor");
        if (statusBarAnchor == null)
        {
            Debug.LogError("StatusBarAnchor child object not found on Enemy!", this.gameObject);
        }

    }

    private void OnEnable()
    {

        _health.OnDied += (isSilentKill) => HandleDeath(isSilentKill, false); // Subscribe to the death event


    }

    private void OnDisable()
    {

        _health.OnDied -= (isSilentKill) => HandleDeath(isSilentKill, false); // Unsubscribe


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
                _ai.OnCastProgressChanged -= _uiController.UpdateCastBar;
            }
        }
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Unregister(this);
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
        _ai.SpawnPosition = transform.position;
        _resistances.Initialize(newConfig); 
        _revealableEntity.Initialize(newConfig);

        _ai.StartAI();
      

    }
    public void LoadConfiguration(EnemyConfigSO loadedConfig)
    {
        this.config = loadedConfig;

        if (_health == null) _health = GetComponent<EnemyHealth>();
        if (Detector == null) Detector = GetComponent<DetectionSystem>();
        if (_resistances == null) _resistances = GetComponent<EnemyResistances>();
        if (_revealableEntity == null) _revealableEntity = GetComponent<RevealableEntity>();
        if (_ai == null) _ai = GetComponent<EnemyAI>();

        _health.Initialize(loadedConfig);
        Detector.Initialize(loadedConfig);
        _resistances.Initialize(loadedConfig);
        _revealableEntity.Initialize(loadedConfig);
        _ai.Config = loadedConfig;
    }
    private void Start()
    {
        if (_isLoadedDead)
        {
            // We pass 'true' for isLoadedFromSave to prevent unregistering.
            HandleDeath(false, true);
            return; // IMPORTANT: Stop the rest of Start() from running.
        }

        //Debug.Log($"--- Enemy.Start() called for {gameObject.name} ---");
        //Debug.Log($"[Enemy] Registering enemy: {gameObject.name} with ID: {_uniqueID.ID}");
        EnemyManager.Instance.RegisterEnemy(this);

        //_navigator.SetSpeed(config.patrolSpeed);

        //Instantiate and set up the status bar
        if (statusBarPrefab != null)
        {
            _statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, transform);
            _statusBarInstance.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            _uiController = _statusBarInstance.GetComponent<EnemyUIController>();

            // Connect the standard events
            if (_uiController != null)
            {
                _health.OnHealthChanged += _uiController.UpdateHealth;
                Detector.OnSoundGaugeChanged += _uiController.UpdateAlert;
                _uiController.InitializeRevealIcon(config.revealIconPrefab);
            }

            if (_ai != null)
            {
                _ai.OnStateChanged += _uiController.HandleAIStateChanged;
                _ai.OnCastProgressChanged += _uiController.UpdateCastBar;
                _uiController.HandleAIStateChanged(_ai.CurrentState);
            }

            // After creating the UI, immediately tell it to update with the initial values.
            _uiController.UpdateHealth(_health.CurrentHealth, config.maxHealth);
            // The initial sound gauge is 0, which will correctly hide the alert slider.
            //_uiController.UpdateAlert(0, config.hearingThreshold);
        }


        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Register(this);
        }
        else
        {
            Debug.LogError($"Could not register {name}, SaveableEntityRegistry.Instance is null!");
        }

    }

    private void HandleDeath(bool isSilentKill, bool isLoadedFromSave = false)
    {
        // The 'isLoadedFromSave' flag prevents a restored dead body from trying
        // to unregister itself from managers it was never added to in this session.

        if (!isLoadedFromSave)
        {
            // This is the original logic for a "live" death that happens during gameplay.
            // It correctly reports the death to the various managers.
            Debug.Log($"<color=cyan>[Enemy]</color> HandleDeath() called for a LIVE enemy. isSilentKill is: {isSilentKill}");
            bool wasInCombat = (_ai.CurrentState is CombatState);

            if (wasInCombat && CombatManager.Instance != null)
            {
                CombatManager.Instance.ReportEnemyDeath(this, isSilentKill);
            }
            else if (CombatManager.Instance != null)
            {
                CombatManager.Instance.UnregisterEnemyFromCombat(this);
            }

            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.UnregisterEnemy(this);
            }
        }

        // This logic runs for BOTH live deaths and loaded dead bodies to ensure
        // they are visually and functionally in a "dead" state.

        if (_uiController != null)
        {
            _health.OnHealthChanged -= _uiController.UpdateHealth;
            Detector.OnSoundGaugeChanged -= _uiController.UpdateAlert;
            if (_ai != null)
            {
                _ai.OnStateChanged -= _uiController.HandleAIStateChanged;
            }
        }
        if (_statusBarInstance != null)
        {
            Destroy(_statusBarInstance);
        }

        // 2. Disable all sensory and intelligence components
        if (Detector != null) Detector.enabled = false;
        if (_ai != null) _ai.enabled = false;
        if (_navigator != null)
        {
            _navigator.enabled = false;
            if (gameObject.activeInHierarchy) // Only call Stop if the object is active and on a NavMesh
            {
                var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent.isOnNavMesh)
                {
                    _navigator.Stop();
                }
            }
        }

        // Play the death animation
        _animController.PlayDeathAnimation();

        // Set the object's tag and layer to identify it as a corpse
        gameObject.tag = "DeadBody";
        gameObject.layer = LayerMask.NameToLayer("DeadBody");
    }

    private IEnumerator CleanupBody(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void HandlePlayerDeath()
    {
        _ai.ClearTarget();
        if (_ai != null) _ai.ResetToInitialState();
        if (Detector != null) Detector.enabled = false;
    }

    // NEW: This is also called by the EnemyManager
    public void HandlePlayerRespawn()
    {
        // Re-enable senses
        //if (Detector != null) Detector.enabled = true;
    }
}