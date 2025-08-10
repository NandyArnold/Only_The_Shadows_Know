// EnemyAI.cs - COMPLETE & CORRECTED

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Public properties for states to access components
    public EnemyConfigSO Config { get; private set; }
    public EnemyNavigator Navigator { get; private set; }
    public DetectionSystem Detector { get; private set; }
    public Transform PlayerTarget => _playerTarget ??= FindPlayerTarget(); // Robust lazy-load
    public EnemyAnimationController AnimController { get; private set; }
  
    public EnemyCombatHandler CombatHandler { get; private set; } 
    public Vector3 LastKnownPlayerPosition { get; set; } // NEW: Stores player position

    public PatrolRouteSO PatrolRoute { get; private set; }

    private EnemyAIState _currentState;
    public EnemyAIState CurrentState => _currentState;

    private EnemyHealth _health;

    private Transform _playerTarget;

    private void Awake()
    {
        // Get references for the states to use
        Config = GetComponent<Enemy>().Config;
        Navigator = GetComponent<EnemyNavigator>();
        Detector = GetComponent<DetectionSystem>();
        _health = GetComponent<EnemyHealth>();
        AnimController = GetComponent<EnemyAnimationController>();
        CombatHandler = GetComponent<EnemyCombatHandler>();

    }

    // NEW: Subscribe to events when the object is enabled.
    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDamaged += HandleDamage;
        }
        
        if (Detector != null)
        {
            Detector.OnSoundDetected += HandleSoundDetected;
        }

        if (CombatManager.Instance != null)
        {
           
            // CHANGE these to use named methods
            CombatManager.Instance.OnCombatStart += HandleAICombatStart;
            CombatManager.Instance.OnCombatEnd += HandleAICombatEnd;
        }
    }

    // NEW: Unsubscribe from events when the object is disabled.
    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamage;
        }
        
        if (Detector != null)
        {
            Detector.OnSoundDetected -= HandleSoundDetected;
        }

        {
            // This will now correctly unsubscribe
            CombatManager.Instance.OnCombatStart -= HandleAICombatStart;
            CombatManager.Instance.OnCombatEnd -= HandleAICombatEnd;
        }
    }

    public void Initialize(EnemyConfigSO newConfig, PatrolRouteSO route)
    {
        Config = newConfig;
        PatrolRoute = route;
    }

    private void Start()
    {
        // The enemy always starts in the Patrol state.
        TransitionToState(new PatrolState(PatrolRoute));



    }

    private void Update()
    {
        //Debug.Log($"Current State: {_currentState?.GetType().Name ?? "None"}", this);
        // Run the current state's logic every frame.
        _currentState?.Execute(this);
    }

    public void TransitionToState(EnemyAIState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    // NEW: This method is called when the enemy is damaged.
    private void HandleDamage(GameObject attacker)
    {
        if (PlayerTarget == null) return;
        LastKnownPlayerPosition = PlayerTarget.position;

        if (Config.instantlyKnowsAttackerLocation)
        {
            TransitionToState(new CombatState());
        }
        else
        {
            TransitionToState(new AlertState(LastKnownPlayerPosition));
        }
    }

    // NEW: This public method can be called by external systems (like CombatManager or a skill)
    public void ForceReturnToPatrol()
    {
        TransitionToState(new PatrolState(PatrolRoute));
    }

    private void HandleSoundDetected(Vector3 soundPosition)
    {
        // We only react to sound if we are peacefully patrolling.
        if (_currentState is PatrolState)
        {
            Debug.Log("Heard a sound! Investigating...");
            TransitionToState(new AlertState(soundPosition));
        }
    }

    private void HandleAICombatStart() => AnimController.SetIsInCombat(true);
    private void HandleAICombatEnd() => AnimController.SetIsInCombat(false);

    private Transform FindPlayerTarget()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            if (GameManager.Instance.Player.IsDead) return null;
            return GameManager.Instance.Player.transform;
        }
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }
}