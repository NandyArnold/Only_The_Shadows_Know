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
    [SerializeField] private PlayerStats playerStats;

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
    [SerializeField] private float runningJumpHeight = 2.5f;
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

    [Header("Data References")] 
    [SerializeField] private NoiseSettingsSO noiseSettings;
    

    private bool _wasGrounded;

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
    private bool _movementLocked = false;
    private bool _isJumping = false;
    private bool _jumpInputBuffered = false;
    private Coroutine _jumpBufferCoroutine;

    private Transform _cameraTransform;

    private Invulnerability _invulnerability;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerCombat = GetComponent<PlayerCombat>();
        _cameraTransform = Camera.main.transform;
        _currentSpeed = walkSpeed;
        if (noiseSettings == null) Debug.LogError("NoiseSettingsSO not assigned on PlayerMovement!");
        _invulnerability = GetComponent<Invulnerability>();
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput -= SetMoveInput;
          
            playerInputHandler.OnCrouchInput -= Crouch;
            playerInputHandler.OnRunInput -= SetRunningState;
            playerInputHandler.OnJumpInput -= Jump;
            playerInputHandler.OnDodgeInput -= PerformDodgeRoll;


            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged -= HandleAimStateChanged;
                playerCombat.OnFocusStateChanged -= HandleFocusStateChanged; 
            }

            playerInputHandler.OnMoveInput += SetMoveInput;

            playerInputHandler.OnJumpInput += Jump;
            playerInputHandler.OnDodgeInput += PerformDodgeRoll;

            playerInputHandler.OnCrouchInput += Crouch;
            playerInputHandler.OnRunInput += SetRunningState;


            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged += HandleAimStateChanged;
                playerCombat.OnFocusStateChanged += HandleFocusStateChanged; 
            }
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMoveInput -= SetMoveInput;

            playerInputHandler.OnJumpInput -= Jump;
            playerInputHandler.OnDodgeInput -= PerformDodgeRoll;

            playerInputHandler.OnCrouchInput -= Crouch;
            playerInputHandler.OnRunInput -= SetRunningState;

            if (playerCombat != null)
            {
                playerCombat.OnAimStateChanged -= HandleAimStateChanged;
                playerCombat.OnFocusStateChanged -= HandleFocusStateChanged; 
            }

        }
    }

    private void Update()
    {
        ApplyGravity();
        HandlePlayerRotation();
        HandleContinuousNoise();

        if (_movementLocked)
        {
            // If movement is locked, stop all animations...
            playerAnimationController.SetLocomotionInput(0, 0, _isCrouching, false);
            // ...apply only the gravity velocity...
            characterController.Move(_velocity * Time.deltaTime);
            // ...and exit the method so HandleMovement() is not called.
            return;
        }

        CheckIfGrounded();
        HandleMovement();
       
    }

    private void CheckIfGrounded()
    {
        bool isGroundedNow = Physics.SphereCast(
            groundCheckPoint.position,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit hitInfo,
            groundCastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!_wasGrounded && isGroundedNow)
        {
            NoiseManager.Instance.GenerateNoise(transform.position, noiseSettings.jumpNoise, this.gameObject);
            _isJumping = false;
        }

        _isGrounded = isGroundedNow;
        _wasGrounded = isGroundedNow;

    }
    // This is now the entry point for a single-press jump


    public void Jump()
    {
        // All of your original guard clauses are here, making it safe.
        if (_isJumping || _isFocused || _isDodgeRolling || !_isGrounded) return;

        _isJumping = true; // Set the lock

        Debug.Log("<color=yellow>ACTION: Jump method called.</color>");

        // Choose which jump height to use based on the isRunning state


        // If the checks above pass, we are clear to jump.
        float currentJumpHeight = _isRunning ? runningJumpHeight : jumpHeight;
        float noiseToGenerate = _isRunning ? noiseSettings.jumpRunningNoise : noiseSettings.jumpNoise;


        _velocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);

        if (_isRunning)
        {
            playerAnimationController.PlayRunningJumpAnimation();
            NoiseManager.Instance.GenerateNoise(transform.position, noiseToGenerate, this.gameObject);
        }
        else
        {
            playerAnimationController.PlayStandardJumpAnimation();
            NoiseManager.Instance.GenerateNoise(transform.position, noiseToGenerate, this.gameObject);
        }
        //_isJumping = false; // Reset jumping state after initiating jump
    }

    public void PerformDodgeRoll()
    {
    

      
        if (_isDodgeRolling || _isJumping || _isFocused || !_isGrounded) return;

        NoiseManager.Instance.GenerateNoise(transform.position, noiseSettings.dodgeRollNoise, gameObject);
        StartCoroutine(DodgeRollCoroutine());
    }

  
    



    private void HandlePlayerRotation()
    {
        if (GameManager.Instance.CurrentState != GameState.Gameplay) return;
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
        try
        {
            if (_invulnerability != null) _invulnerability.IsInvulnerable = true;
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
        }
        finally
        {
            if (_invulnerability != null) _invulnerability.IsInvulnerable = false;
            _isDodgeRolling = false;    
        }
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

    private void HandleContinuousNoise()
    {
        // If we are moving
        if (_moveInput.magnitude > 0.1f)
        {
            if (_isCrouching)
            {
                // Generate crouch-walk noise (e.g., 25 * 0.5 = 12.5 per second)
                float crouchNoise = noiseSettings.walkNoise * noiseSettings.crouchNoiseModifier;
                NoiseManager.Instance.GenerateNoise(transform.position, crouchNoise * Time.deltaTime, gameObject);
            }
            else if (_isRunning)
            {
                // Generate run noise
                NoiseManager.Instance.GenerateNoise(transform.position, noiseSettings.runNoise * Time.deltaTime, gameObject);
            }
            else // Must be walking
            {
                // Generate walk noise
                NoiseManager.Instance.GenerateNoise(transform.position, noiseSettings.walkNoise * Time.deltaTime, gameObject);
            }
        }
    }

    public void SetMovementLock(bool isLocked)
    {
        _movementLocked = isLocked;
    }

   
}