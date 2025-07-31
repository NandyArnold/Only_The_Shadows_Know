using UnityEngine;

// Enum for basic locomotion states (used internally by PlayerMovement for clarity)
// PlayerAnimationController will use more granular direct parameter setting
public enum LocomotionState { Idle, Walk, Run, CrouchWalk, Jump, DodgeRoll }

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private PlayerInputHandler playerInputHandler; // Added reference

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; // E.g., 50% of walk speed when crouching
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f; // Standard Earth gravity

    [Header("Character Controller Settings")]
    [SerializeField] private float normalHeight = 2.0f; // Default CharacterController height
    [SerializeField] private float normalCenterY = 1.0f; // Default CharacterController center Y
    [SerializeField] private float crouchHeight = 1.0f;  // Crouched CharacterController height
    [SerializeField] private float crouchCenterY = 0.5f; // Crouched CharacterController center Y

    private Vector2 _moveInput;
    private Vector3 _velocity; // Current velocity, especially for gravity
    private bool _isCrouching;
    private bool _isRunning; // To track if Shift is held
    private float _currentSpeed; // The actual speed player is moving at

    private Transform _cameraTransform; // To orient movement relative to camera

    private void Awake()
    {
        // Get references if not set in Inspector
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (playerAnimationController == null)
            playerAnimationController = GetComponent<PlayerAnimationController>();
        if (playerInputHandler == null) // Get PlayerInputHandler from the same GameObject
            playerInputHandler = GetComponent<PlayerInputHandler>();

        _cameraTransform = Camera.main.transform; // Assuming main camera is tagged "MainCamera"

        _currentSpeed = walkSpeed; // Start at walk speed
    }

    private void OnEnable()
    {
        // Subscribe to input events
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput += SetMoveInput;
            playerInputHandler.OnJumpInput += Jump;
            playerInputHandler.OnCrouchInput += Crouch; // Use a toggle for Crouch
            playerInputHandler.OnRunInput += SetRunningState; // Use a boolean for run (e.g., from Shift key held)
            // playerInputHandler.OnDodgeRollInput += DodgeRoll; // Uncomment when ready to implement
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from input events to prevent memory leaks
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput -= SetMoveInput;
            playerInputHandler.OnJumpInput -= Jump;
            playerInputHandler.OnCrouchInput -= Crouch;
            playerInputHandler.OnRunInput -= SetRunningState;
            // playerInputHandler.OnDodgeRollInput -= DodgeRoll;
        }
    }

    private void Update()
    {
        ApplyGravity();
        HandleMovement();
        Debug.Log("Is Grounded: " + characterController.isGrounded + ", Velocity Y: " + _velocity.y);
    
    }

    // --- Input Handling Callbacks (Called by PlayerInputHandler) ---

    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input.normalized; // Normalize to ensure consistent speed for diagonals
    }

    public void SetRunningState(bool isRunning)
    {
        _isRunning = isRunning;
        UpdateCurrentSpeed();
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            playerAnimationController.PlayJumpAnimation(); // Trigger jump animation
            Debug.Log("Jump!"); // For testing
        }
    }

    public void Crouch(bool isCrouching) // This will be a toggle from Input Handler
    {
        _isCrouching = isCrouching;
        // Adjust CharacterController height and center immediately
        characterController.height = _isCrouching ? crouchHeight : normalHeight;
        characterController.center = _isCrouching ? new Vector3(0, crouchCenterY, 0) : new Vector3(0, normalCenterY, 0);

        UpdateCurrentSpeed(); // Recalculate speed based on crouching state

        Debug.Log($"Crouching: {_isCrouching}"); // For testing
    }

    // Add placeholders for other actions
    public void DodgeRoll()
    {
        Debug.Log("Dodge Roll (Not Implemented Yet)");
        playerAnimationController.PlayDodgeRollAnimation();
    }

    public void LedgeTeleport(Vector3 targetPos)
    {
        Debug.Log("Ledge Teleport (Not Implemented Yet)");
        // Logic for teleportation will go here, using TeleportManager
    }


    // --- Internal Movement Logic ---

    private void UpdateCurrentSpeed()
    {
        if (_isCrouching)
        {
            _currentSpeed = walkSpeed * crouchSpeedMultiplier;
        }
        else
        {
            _currentSpeed = _isRunning ? runSpeed : walkSpeed;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small negative value to keep player "grounded"
        }
        _velocity.y += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        // Calculate movement direction relative to camera's forward/right
        Vector3 cameraForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
        Vector3 moveDirection = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        // Apply movement
        if (_moveInput.magnitude > 0.1f) // Only move if there's significant input
        {
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _currentSpeed * 10f * Time.deltaTime); // Adjust rotation speed as needed

            characterController.Move(moveDirection * _currentSpeed * Time.deltaTime + _velocity * Time.deltaTime);
        }
        else
        {
            characterController.Move(_velocity * Time.deltaTime); // Only apply gravity if no horizontal input
        }

        // --- Update Animator Parameters ---
        // Pass normalized input directly to the Animator's MoveX and MoveY parameters.
        // We'll also pass IsCrouching and IsRunning state for transitions and blend tree adjustments.
        playerAnimationController.SetLocomotionInput(_moveInput.x, _moveInput.y, _isCrouching, _isRunning);
    }
}