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
        //Debug.Log("Is Grounded: " + characterController.isGrounded + ", Velocity Y: " + _velocity.y);
    
    }

    // Inside PlayerMovement.cs, after the Update() method

    private void LateUpdate()
    {
        HandlePlayerRotation();
    }

    private void HandlePlayerRotation()
    {
        //if (_cameraTransform == null)
        //{
        //    _cameraTransform = Camera.main.transform; // Ensure camera reference is set
        //    if (_cameraTransform == null) return;
        //}

        //// Get the camera's forward direction, but only on the XZ plane (ignore Y)
        //Vector3 cameraForwardFlat = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;

        //// Rotate the player to match the camera's horizontal forward direction
        //if (cameraForwardFlat != Vector3.zero) // Avoid LookRotation with zero vector
        //{
        //    transform.rotation = Quaternion.Slerp(
        //        transform.rotation,
        //        Quaternion.LookRotation(cameraForwardFlat),
        //        _currentSpeed * 10f * Time.deltaTime // Adjust this rotation speed if player turns too fast/slow
        //    );
        //}

        if (_cameraTransform == null) return;

        // Make the player's forward direction match the camera's horizontal (yaw) direction.
        transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
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
        // Movement is now relative to the player's own forward/right, which is aligned with the camera.
        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        // Apply movement
        characterController.Move(moveDirection * _currentSpeed * Time.deltaTime);
        characterController.Move(_velocity * Time.deltaTime); // Apply gravity separately

        // --- Update Animator Parameters ---
        float animSpeedMultiplier = _isRunning && !_isCrouching ? runSpeed / walkSpeed : 1f;

        // Pass LOCAL input to the animator. Since the player is always facing forward relative to the camera,
        // _moveInput.y is forward/backward and _moveInput.x is strafing left/right.
        playerAnimationController.SetLocomotionInput(
            _moveInput.x * animSpeedMultiplier,
            _moveInput.y * animSpeedMultiplier,
            _isCrouching,
            _isRunning
        );
        //// Calculate movement direction relative to camera's forward/right
        //Vector3 cameraForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        //Vector3 cameraRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
        //Vector3 moveDirection = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        //// Apply movement
        //if (_moveInput.magnitude > 0.1f) // Only move if there's significant input
        //{
        //    // Rotate player to face movement direction
        //    //Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        //    //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _currentSpeed * 10f * Time.deltaTime); // Adjust rotation speed as needed

        //    characterController.Move(moveDirection * _currentSpeed * Time.deltaTime + _velocity * Time.deltaTime);
        //}
        //else
        //{
        //    characterController.Move(_velocity * Time.deltaTime); // Only apply gravity if no horizontal input
        //}

        //// --- Update Animator Parameters ---
        //// Pass normalized input directly to the Animator's MoveX and MoveY parameters.
        //// We'll also pass IsCrouching and IsRunning state for transitions and blend tree adjustments.
        //// Scale the input based on whether the character is running
        //float animSpeedMultiplier = 1f;
        //if (_isRunning && !_isCrouching) // Only multiply if actually running (not crouching)
        //{
        //    animSpeedMultiplier = runSpeed / walkSpeed; // e.g., 6/3 = 2
        //}
        //else if (_isCrouching)
        //{
        //    animSpeedMultiplier = crouchSpeedMultiplier; // e.g., 0.5
        //}


        //// Pass scaled input to animation controller
        //playerAnimationController.SetLocomotionInput(
        //    _moveInput.x * animSpeedMultiplier, // Scale X input
        //    _moveInput.y * animSpeedMultiplier, // Scale Y input
        //    _isCrouching,
        //    _isRunning // Still pass this if you want to use it for other logic later
        //);
    }
}