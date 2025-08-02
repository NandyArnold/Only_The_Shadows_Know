using UnityEngine;
using System.Collections;

public enum LocomotionState { Idle, Walk, Run, CrouchWalk, Jump, DodgeRoll }

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCastDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float aimSpeedMultiplier = 0.5f;

    [Header("Character Controller Settings")]
    [SerializeField] private float normalHeight = 2.0f;
    [SerializeField] private float normalCenterY = 1.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchCenterY = 0.5f;

    [Header("Dodge Roll Settings")]
    [SerializeField] private float dodgeRollSpeed = 10f;
    [SerializeField] private float dodgeRollDuration = 0.5f;

    private Vector2 _moveInput;
    private Vector2 _smoothedMoveInput;
    [SerializeField] private float animationSmoothTime = 0.1f;

    private Vector3 _velocity;
    private bool _isCrouching;
    private bool _isRunning;
    private float _currentSpeed;
    private bool _isAiming;
    private bool _isDodgeRolling = false;
    private bool _isGrounded;
    private bool _isFocused;

    private Transform _cameraTransform;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerCombat = GetComponent<PlayerCombat>();
        _cameraTransform = Camera.main.transform;
        _currentSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput -= SetMoveInput;
            playerInputHandler.OnJumpInput -= Jump;
            playerInputHandler.OnCrouchInput -= Crouch;
            playerInputHandler.OnRunInput -= SetRunningState;

            playerInputHandler.OnDodgeRollInput -= HandleDodgeRoll;

            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged -= HandleAimStateChanged;
                playerCombat.OnFocusStateChanged -= HandleFocusStateChanged; // NEW: Unsubscribe
            }

            playerInputHandler.OnMoveInput += SetMoveInput;
            playerInputHandler.OnJumpInput += Jump;
            playerInputHandler.OnCrouchInput += Crouch;
            playerInputHandler.OnRunInput += SetRunningState;

            playerInputHandler.OnDodgeRollInput += HandleDodgeRoll;

            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged += HandleAimStateChanged;
                playerCombat.OnFocusStateChanged += HandleFocusStateChanged; // NEW: Subscribe
            }
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput -= SetMoveInput;
            playerInputHandler.OnJumpInput -= Jump;
            playerInputHandler.OnCrouchInput -= Crouch;
            playerInputHandler.OnRunInput -= SetRunningState;

            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged -= HandleAimStateChanged;
                playerCombat.OnFocusStateChanged -= HandleFocusStateChanged; // NEW: Unsubscribe
            }

            playerInputHandler.OnDodgeRollInput -= HandleDodgeRoll;
        }
    }

    private void Update()
    {
        CheckIfGrounded();
        ApplyGravity();
        HandleMovement();
        HandlePlayerRotation();
    }

    private void CheckIfGrounded()
    {
        _isGrounded = Physics.SphereCast(
            groundCheckPoint.position,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit hitInfo,
            groundCastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    public void Jump()
    {
        Debug.Log("<color=yellow>ACTION: Jump method called.</color>");

        if (_isFocused)
        {
            Debug.Log("<color=red>JUMP BLOCKED: Is Focused.</color>");
            return;
        }

        if (_isDodgeRolling)
        {
            Debug.Log("<color=red>JUMP BLOCKED: Is Dodge Rolling.</color>");
            return;
        }

        
        // We check if the player is NOT grounded and return.
        if (!_isGrounded)
        {
            Debug.Log("<color=red>JUMP BLOCKED: Not Grounded.</color>");
            return;
        }

        // If the checks above pass, we are clear to jump.
        _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        if (_isRunning)
        {
            playerAnimationController.PlayRunningJumpAnimation();
        }
        else
        {
            playerAnimationController.PlayStandardJumpAnimation();
        }
    }

    private void HandleDodgeRoll()
    {
        Debug.Log("<color=yellow>ACTION: HandleDodgeRoll method called.</color>");
        if (_isDodgeRolling)
        {
            Debug.Log("<color=red>DODGE BLOCKED: Already Dodge Rolling.</color>");
            return;
        }

        // --- THIS IS THE FIX ---
        // We check if the player is NOT grounded and return.
        if (!_isGrounded)
        {
            Debug.Log("<color=red>DODGE BLOCKED: Not Grounded.</color>");
            return;
        }

        StartCoroutine(DodgeRollCoroutine());
    }

    // ... (The rest of the script is unchanged) ...

    #region Unchanged Methods
    private void HandlePlayerRotation()
    {
        if (_cameraTransform == null) return;
        transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
    }

    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input.normalized;
    }

    public void SetRunningState(bool isRunning)
    {
        _isRunning = isRunning;
        UpdateCurrentSpeed();
    }

    public void Crouch(bool isCrouching)
    {
        _isCrouching = isCrouching;
        characterController.height = _isCrouching ? crouchHeight : normalHeight;
        characterController.center = _isCrouching ? new Vector3(0, crouchCenterY, 0) : new Vector3(0, normalCenterY, 0);
        UpdateCurrentSpeed();
    }

    private void HandleAimStateChanged(bool isAiming)
    {
        _isAiming = isAiming;
        UpdateCurrentSpeed();
    }

    private void UpdateCurrentSpeed()
    {
        if (_isCrouching)
        {
            _currentSpeed = walkSpeed * crouchSpeedMultiplier;
        }
        else if (_isAiming)
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
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        _velocity.y += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        characterController.Move(moveDirection * _currentSpeed * Time.deltaTime);
        characterController.Move(_velocity * Time.deltaTime);

        _smoothedMoveInput = Vector2.Lerp(_smoothedMoveInput, _moveInput, Time.deltaTime / animationSmoothTime);

        float animSpeedMultiplier = _isRunning && !_isCrouching ? runSpeed / walkSpeed : 1f;
        playerAnimationController.SetLocomotionInput(
            _smoothedMoveInput.x * animSpeedMultiplier,
            _smoothedMoveInput.y * animSpeedMultiplier,
            _isCrouching,
            _isRunning
        );
    }

    private void HandleFocusStateChanged(bool isFocused)
    {
        _isFocused = isFocused;
    }

    private IEnumerator DodgeRollCoroutine()
    {
        _isDodgeRolling = true;
        playerAnimationController.PlayDodgeRollAnimation();

        float startTime = Time.time;
        Vector3 rollDirection = _moveInput.magnitude > 0.1f
            ? (transform.forward * _moveInput.y + transform.right * _moveInput.x).normalized
            : transform.forward;

        rollDirection.y = 0;
        rollDirection = rollDirection.normalized;

        while (Time.time < startTime + dodgeRollDuration)
        {
            characterController.Move(rollDirection * dodgeRollSpeed * Time.deltaTime);
            yield return null;
        }

        _isDodgeRolling = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = Color.red;
        // Draw the starting sphere
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        // Draw a line showing the cast path and distance
        Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCastDistance);
    }
    #endregion
}