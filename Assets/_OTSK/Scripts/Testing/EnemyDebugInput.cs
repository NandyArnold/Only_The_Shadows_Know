// EnemyDebugInput.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyAnimationController))]
public class EnemyDebugInput : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    private NavMeshAgent _agent;
    private EnemyAnimationController _animController;
    private PlayerInputActions _inputActions;

    private Vector2 _moveInput;
    private bool _isRunning;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<EnemyAnimationController>();
        _inputActions = new PlayerInputActions();

        // Set up callbacks for the "EnemyDebug" map
        _inputActions.EnemyDebug.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.EnemyDebug.Move.canceled += ctx => _moveInput = Vector2.zero;
        _inputActions.EnemyDebug.Run.performed += ctx => _isRunning = true;
        _inputActions.EnemyDebug.Run.canceled += ctx => _isRunning = false;
        _inputActions.EnemyDebug.PrimaryAttack.performed += ctx => _animController.PlayDeathAnimation(); // For testing an animation
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
        float currentSpeed = _isRunning ? runSpeed : walkSpeed;
        Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        // Directly control the agent's velocity for responsive movement
        _agent.velocity = moveDirection * currentSpeed;

        // TODO: Update animator with a "Speed" parameter for a locomotion blend tree.
        // _animController.SetSpeed(_agent.velocity.magnitude); 
    }
}