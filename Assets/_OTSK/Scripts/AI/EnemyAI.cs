// EnemyAI.cs - COMPLETE & CORRECTED

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Public properties for states to access components
    public EnemyConfigSO Config { get; private set; }
    public EnemyNavigator Navigator { get; private set; }
    public DetectionSystem Detector { get; private set; }
    public Transform PlayerTarget { get; private set; }
    public PatrolRouteSO PatrolRoute => patrolRoute; // NEW: Exposes the route for other states
    public Vector3 LastKnownPlayerPosition { get; set; } // NEW: Stores player position

    [Header("AI Data")]
    [SerializeField] private PatrolRouteSO patrolRoute;

    private EnemyAIState _currentState;
    private EnemyHealth _health;

    private void Awake()
    {
        // Get references for the states to use
        Config = GetComponent<Enemy>().Config;
        Navigator = GetComponent<EnemyNavigator>();
        Detector = GetComponent<Enemy>().Detector;
        _health = GetComponent<EnemyHealth>();

        // Find the player
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            PlayerTarget = GameManager.Instance.Player.transform;
        }
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
    }

    private void Start()
    {
        // The enemy always starts in the Patrol state.
        TransitionToState(new PatrolState(patrolRoute));
    }

    private void Update()
    {
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
        LastKnownPlayerPosition = attacker.transform.position;

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
        TransitionToState(new PatrolState(patrolRoute));
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
}