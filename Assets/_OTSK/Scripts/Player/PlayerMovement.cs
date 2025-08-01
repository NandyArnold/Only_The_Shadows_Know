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
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; // E.g., 50% of walk speed when crouching
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f; // Standard Earth gravity
    [SerializeField] private float aimSpeedMultiplier = 0.5f;

    [Header("Character Controller Settings")]
    [SerializeField] private float normalHeight = 2.0f; // Default CharacterController height
    [SerializeField] private float normalCenterY = 1.0f; // Default CharacterController center Y
    [SerializeField] private float crouchHeight = 1.0f;  // Crouched CharacterController height
    [SerializeField] private float crouchCenterY = 0.5f; // Crouched CharacterController center Y

    private Vector2 _moveInput;
    private Vector2 _smoothedMoveInput;
    [SerializeField] private float animationSmoothTime = 0.1f;

    private Vector3 _velocity; // Current velocity, especially for gravity
    private bool _isCrouching;
    private bool _isRunning; // To track if Shift is held
    private float _currentSpeed; // The actual speed player is moving at
    private bool _isAiming;

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
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();
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
            if (playerCombat != null)
                playerCombat.OnAimStateChanged += HandleAimStateChanged;
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
            if (playerCombat != null)
                playerCombat.OnAimStateChanged -= HandleAimStateChanged;
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

    private void HandleAimStateChanged(bool isAiming)
    {
        _isAiming = isAiming;
        UpdateCurrentSpeed(); // Recalculate speed immediately.
    }

    private void UpdateCurrentSpeed()
    {
        if (_isCrouching)
        {
            _currentSpeed = walkSpeed * crouchSpeedMultiplier;
        }
        else if (_isAiming) // Add this new condition
        {
            _currentSpeed = walkSpeed * aimSpeedMultiplier;
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

        // Smooth the raw input over time.
        _smoothedMoveInput = Vector2.Lerp(_smoothedMoveInput, _moveInput, Time.deltaTime / animationSmoothTime);

        // --- Update Animator Parameters ---
        float animSpeedMultiplier = _isRunning && !_isCrouching ? runSpeed / walkSpeed : 1f;

        // Pass LOCAL input to the animator. Since the player is always facing forward relative to the camera,
        // _moveInput.y is forward/backward and _moveInput.x is strafing left/right.
        playerAnimationController.SetLocomotionInput(
            _smoothedMoveInput.x * animSpeedMultiplier, // Use smoothed value
            _smoothedMoveInput.y * animSpeedMultiplier, // Use smoothed value
            _isCrouching,
            _isRunning
        );
        
    }
 
}