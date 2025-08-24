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
        public bool isDead;
    }
    // --- ISaveable Implementation ---
    public string UniqueID => _uniqueID.ID;

    private bool _isLoadedDead = false;
    private bool _isRegistered = false;

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

        bool isActuallyDead = _health.IsDead;

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
            currentHealth = isActuallyDead ? 0f : _health.CurrentHealth,
            isDead = isActuallyDead,

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

        if (saveData.isDead)
        {
            _isLoadedDead = true;
            return;
        }

        if (!saveData.wasActive || saveData.currentHealth <= 0)
        {
            _isLoadedDead = true; // Set the flag
            return; 
        }


        transform.position = new Vector3(saveData.posX, saveData.posY, saveData.posZ);
        transform.rotation = new Quaternion(saveData.rotX, saveData.rotY, saveData.rotZ, saveData.rotW);

        // Restore variable stats
        _health.SetHealth(saveData.currentHealth);
        _ai.SetSummonCount(saveData.summonCount);
        _ai.InitialState = saveData.initialState; // Set the intended initial state

        // Now that health is restored, update the UI with initial values.
        if (_uiController != null)
        {
            _uiController.UpdateHealth(_health.CurrentHealth, config.maxHealth);
            _uiController.HandleAIStateChanged(_ai.CurrentState); // Set initial AI icon

            // --- THIS IS THE FINAL FIX ---
            // Explicitly tell the alert bar to initialize its state.
            // This mirrors the logic in DetectionSystem.Start() and ensures
            // the UI is correctly set to zero when the enemy is loaded.
            _uiController.UpdateAlert(0, config.hearingThreshold);
        }
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

   
    private void OnDestroy()
    {
        //Debug.Log($"<color=purple>OnDestroy called for {config.displayName} with ID {UniqueID}</color>");
        UnsubscribeEvents();
        if (_isRegistered)
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.UnregisterEnemy(this);
            }

            // ADD THIS: Notify the combat manager that this enemy is gone.
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.UnregisterEnemyFromCombat(this);
            }

         

            {
                if (SaveableEntityRegistry.Instance != null)
                {
                    SaveableEntityRegistry.Instance.Unregister(this);
                }
                // ... other manager unregistrations
            }
        //Debug.Log($"<color=white>OnDestroy completed for {name}</color>");
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
   
    }

    private void HandleDeath(bool isSilentKill, bool isLoadedFromSave = false)
    {
        // The 'isLoadedFromSave' flag prevents a restored dead body from trying
        // to unregister itself from managers it was never added to in this session.
        _health.SetDeadState();

       

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
        if (TryGetComponent<EnemyCombatHandler>(out var combatHandler))
        {
            combatHandler.enabled = false;
        }

        //UnsubscribeEvents();

        if (_statusBarInstance != null)
        {
            Destroy(_statusBarInstance);
        }

        _ai?.CurrentState?.Exit(_ai);
        if (_ai != null && _ai.enabled)
        {
            _ai.TransitionToState(new DeathState());
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
        SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("DeadBody"));
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

    /// <summary>
    /// Recursively sets the layer of a GameObject and all of its children.
    /// </summary>
    /// <param name="obj">The parent GameObject.</param>
    /// <param name="newLayer">The integer ID of the new layer.</param>
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void SetupForLoad(EnemyConfigSO loadedConfig, PatrolRoute route)
    {
        // 1. Manually gather component references (from Awake)
        Detector = GetComponent<DetectionSystem>();
        _health = GetComponent<EnemyHealth>();
        _navigator = GetComponent<EnemyNavigator>();
        _ai = GetComponent<EnemyAI>();
        _animController = GetComponent<EnemyAnimationController>();
        _collider = GetComponent<CapsuleCollider>();
        _uniqueID = GetComponent<UniqueID>();
        _resistances = GetComponent<EnemyResistances>();
        _revealableEntity = GetComponent<RevealableEntity>();
        statusBarAnchor = transform.Find("StatusBarAnchor");
        revealIconAnchor = transform.Find("RevealIcon_Anchor");

       

        // 2. Apply the configuration (from Initialize/LoadConfiguration)
        this.config = loadedConfig;
        _health.Initialize(config);
        Detector.Initialize(config);
        _ai.Config = config;
        _ai.PatrolRoute = route; // Assign the patrol route
        _resistances.Initialize(config);
        _revealableEntity.Initialize(config);

        EnemyManager.Instance.RegisterEnemy(this);

        if (statusBarPrefab != null && _statusBarInstance == null)
        {
            _statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, transform);
            _uiController = _statusBarInstance.GetComponent<EnemyUIController>();

            _uiController.Initialize(_ai, Detector, _health, config);
        }

        SubscribeEvents();

        // Register with the save system.
        if (SaveableEntityRegistry.Instance != null)
        {
            _isRegistered = SaveableEntityRegistry.Instance.Register(this);
            // We don't need to self-destruct here because the SaveLoadManager handles the loop.
        }
    }

    public void ActivateAIFromLoad(Enemy.EnemySaveData saveData)
    {
        // Now that the enemy is fully configured and its state is restored, start the AI.
        // We pass the saved waypoint index to the AI's Start method.
        _ai.StartAI(saveData.lastWaypointIndex);
    }
    private void HandleDeathEvent(bool isSilentKill)
    {
        HandleDeath(isSilentKill, false);
    }

    public void PostAwakeInitialize()
    {
        // This code now runs for a freshly spawned enemy in a NEW GAME,
        // after we know all Awake() methods are complete.
        EnemyManager.Instance.RegisterEnemy(this);

        if (statusBarPrefab != null && _statusBarInstance == null)
        {
            _statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, transform);
            _uiController = _statusBarInstance.GetComponent<EnemyUIController>();
            _uiController.Initialize(_ai, Detector, _health, config);
        }
        SubscribeEvents();


        if (_uiController != null)
        {
            _uiController.UpdateHealth(_health.CurrentHealth, config.maxHealth);
            _uiController.HandleAIStateChanged(_ai.CurrentState);
        }

        if (SaveableEntityRegistry.Instance != null)
        {
            _isRegistered = SaveableEntityRegistry.Instance.Register(this);
            if (!_isRegistered)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
    private void SubscribeEvents()
    {
        // 1. The wire that tells this Enemy script to handle death when health runs out.
        if (_health != null) _health.OnDied += HandleDeathEvent;

        // 2. The wire that tells the AI to listen to the Detection system.
        if (Detector != null && _ai != null)
        {
            Detector.OnSoundDetected += _ai.HandleSoundDetected;
            Detector.OnDeadBodySpotted += _ai.HandleDeadBodySpotted;
        }
    }

    private void UnsubscribeEvents()
    {
        if (_health != null) _health.OnDied -= HandleDeathEvent;

        if (Detector != null && _ai != null)
        {
            Detector.OnSoundDetected -= _ai.HandleSoundDetected;
            Detector.OnDeadBodySpotted -= _ai.HandleDeadBodySpotted;
        }
    }



}