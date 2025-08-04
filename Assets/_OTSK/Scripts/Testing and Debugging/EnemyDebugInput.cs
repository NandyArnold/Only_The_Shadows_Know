// EnemyDebugInput.cs - UPGRADED with Config Speeds
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyAnimationController))]
public class EnemyDebugInput : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private EnemyConfigSO config; // NEW: Reference to the config file

    // We no longer need local speed variables.

    private NavMeshAgent _agent;
    private EnemyAnimationController _animController;
    private PlayerInputActions _inputActions;

    private Vector2 _moveInput;
    private bool _isInCombat = false;

    // NEW: Enum and state to track which speed we are testing
    private enum DebugSpeedState { Patrol, Alert, CombatWalk, Chase }
    private DebugSpeedState _currentSpeedState = DebugSpeedState.Patrol;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<EnemyAnimationController>();
        _inputActions = new PlayerInputActions();

        _inputActions.EnemyDebug.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.EnemyDebug.Move.canceled += ctx => _moveInput = Vector2.zero;
        _inputActions.EnemyDebug.PrimaryAttack.performed += ctx => _animController.PlayPrimaryAttackAnimation();
        _inputActions.EnemyDebug.ToggleCombat.performed += ctx => ToggleCombatState();
        _inputActions.EnemyDebug.CycleSpeed.performed += ctx => CycleSpeedState();
    }

    private void OnEnable()
    {
        _inputActions.EnemyDebug.Enable();
    }

    private void OnDisable()
    {
        _inputActions.EnemyDebug.Disable();
    }

    private void Update()
    {

        // Get the speed from the config file based on our test state.
        float currentSpeed = GetCurrentSpeedFromConfig();

        // Calculate the direction from WASD input.
        Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        // Set the agent's velocity based on input and the selected speed.
        _agent.velocity = moveDirection * currentSpeed;

        // Only rotate the character if they are moving.
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_agent.velocity.normalized);
        }

        // Update the animator with the agent's actual speed.
        _animController.SetSpeed(_agent.velocity.magnitude);
    }

    private void ToggleCombatState()
    {
        _isInCombat = !_isInCombat;
        _animController.SetIsInCombat(_isInCombat);
        Debug.Log("Toggled Combat State to: " + _isInCombat);
    }

    // NEW: Cycles through the speed states
    private void CycleSpeedState()
    {
        _currentSpeedState = (DebugSpeedState)(((int)_currentSpeedState + 1) % 4);
        Debug.Log("Switched to Speed State: " + _currentSpeedState.ToString());
    }

    // NEW: Returns the correct speed value from the config file
    private float GetCurrentSpeedFromConfig()
    {
        if (config == null) return 2f; // Default value if no config

        switch (_currentSpeedState)
        {
            case DebugSpeedState.Patrol:
                return config.patrolSpeed;
            case DebugSpeedState.Alert:
                return config.alertSpeed;
            case DebugSpeedState.CombatWalk:
                return config.walkCombatSpeed;
            case DebugSpeedState.Chase:
                return config.chaseSpeed;
            default:
                return config.patrolSpeed;
        }
    }
}