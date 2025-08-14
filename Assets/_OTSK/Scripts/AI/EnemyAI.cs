// EnemyAI.cs - COMPLETE & CORRECTED

using System;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    // Public properties for states to access components
    public EnemyConfigSO Config { get; set; }
    public EnemyNavigator Navigator { get; private set; }
    public DetectionSystem Detector { get; private set; }
    public Transform PlayerTarget => _playerTarget ??= FindPlayerTarget(); // Robust lazy-load
    public EnemyAnimationController AnimController { get; private set; }

    public Vector3 SpawnPosition { get; set; }
    public InitialAIState InitialState { get; set; }
   
    public EnemyCombatHandler CombatHandler { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; set; } // NEW: Stores player position

    public PatrolRoute PatrolRoute { get; set; }

    private EnemyAIState _currentState;
    public EnemyAIState CurrentState => _currentState;

    private EnemyHealth _health;

    private Transform _playerTarget;

    public event Action<EnemyAIState> OnStateChanged;

    private void Awake()
    {
        // Get references for the states to use
        //Config = GetComponent<Enemy>().Config;
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
            Detector.OnDeadBodySpotted += HandleDeadBodySpotted;
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
            Detector.OnDeadBodySpotted -= HandleDeadBodySpotted;
        }

    }



    private void Start()
    {

    }

    public void StartAI()
    {
        TransitionToState(new SpawnState());
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
        OnStateChanged?.Invoke(_currentState);
    }

    // NEW: This method is called when the enemy is damaged.
    private void HandleDamage(GameObject attacker)
    {
        if (CurrentState is CombatState || CurrentState is AlertState || CurrentState is AlarmState)
        {
            return;
        }
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
    //public void ForceReturnToPatrol()
    //{
    //    if (CurrentState is PatrolState)
    //    {
    //        return;
    //    }
    //    Navigator.Resume();
    //    TransitionToState(new PatrolState(PatrolRoute));
    //}
    public void ResetToInitialState()
    {
        // Re-enable the NavMeshAgent in case it was disabled (e.g., on death).
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = true;

        var navigator = GetComponent<EnemyNavigator>();
        if (navigator != null) navigator.enabled = true;

        // Use the same logic as StartAI to choose the correct state.
        if (PatrolRoute != null)
        {
            TransitionToState(new PatrolState(PatrolRoute));
        }
        else
        {
            TransitionToState(new GuardState());
        }
    }

    private void HandleSoundDetected(Vector3 soundPosition)
    {
        // We only react to sound if we are peacefully patrolling.
        if (_currentState is PatrolState || _currentState is GuardState)
        {
            Debug.Log("Heard a sound! Investigating...");
            TransitionToState(new AlertState(soundPosition));
        }
    }



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

    public void ClearTarget()
    {
        _playerTarget = null;
    }

    private void HandleDeadBodySpotted(Transform bodyTransform)
    {
        // We should react to a dead body unless we are already in full combat or already raising an alarm.
        if (CurrentState is CombatState || CurrentState is AlarmState) return;

        // If we are in Patrol OR Alert, we should escalate to the AlarmState.
        Debug.Log($"<color=red>{gameObject.name} has spotted a dead body! Sounding the alarm!</color>");
        LastKnownPlayerPosition = bodyTransform.position; // Store the body's location
        TransitionToState(new AlarmState());
    }
}