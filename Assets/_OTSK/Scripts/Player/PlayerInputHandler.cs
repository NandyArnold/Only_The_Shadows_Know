using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System
using System; // Required for Action events

// IMPORTANT: Make sure your PlayerInputActions.Inputactions asset is set to "Generate C# Class"
// And that the generated class name matches what you expect (e.g., PlayerInputActions)

public class PlayerInputHandler : MonoBehaviour
{
    // --- Public Events for other scripts to subscribe to ---
    // PlayerMovement will subscribe to these
    public event Action<Vector2> OnMoveInput;
    public event Action OnJumpInput;
    public event Action<bool> OnCrouchInput; // true for crouch, false for stand
    public event Action<bool> OnRunInput; // true for run (e.g., shift held), false for walk
    public event Action<Vector2> OnLookInput;

    // PlayerCombat will subscribe to these later
    public event Action OnPrimaryAttackInput;
    public event Action OnSecondaryAttackInput;

    // PlayerSkillController will subscribe to these later
    public event Action OnSkill1Input; // E.g., for Spectral Shift
    public event Action OnSkill2Input; // E.g., for Scrying
    public event Action OnSkill3Input; // E.g., for DeathZone

    // Other inputs
    public event Action OnDodgeRollInput;
    public event Action OnInteractInput;
    public event Action OnPauseInput; // For Pause Menu

    // --- Internal References ---
    private PlayerInputActions _inputActions;
    private PlayerMovement _playerMovement; // Direct reference for now, will use events later too
    

    // --- State Variables ---
    private bool _isCrouchToggleActive = false; // For toggling crouch behavior

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _playerMovement = GetComponent<PlayerMovement>(); // Get reference to PlayerMovement on same GameObject

        if (_playerMovement == null)
        {
            Debug.LogError("PlayerInputHandler: PlayerMovement component not found on this GameObject!", this);
        }

        SetupInputCallbacks(); // Configure what happens when input actions are performed
    }

  

    private void OnEnable()
    {
        _inputActions.Enable(); // Enable all input action maps (e.g., "Player" map)
    }

    private void OnDisable()
    {
        //_inputActions.Disable(); // Disable all input action maps
        // The OnDisable logic remains the same.
       
    }

    private void SetupInputCallbacks()
    {
        // --- Player Movement Actions ---
        _inputActions.Player.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.Player.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero); // Send zero when movement stops

        _inputActions.Player.Jump.performed += ctx => OnJumpInput?.Invoke();

        // For Crouch, assuming it's a toggle (like a 'C' key press)
        _inputActions.Player.Crouch.performed += ctx =>
        {
            _isCrouchToggleActive = !_isCrouchToggleActive;
            OnCrouchInput?.Invoke(_isCrouchToggleActive);
        };
        // For Crouch, if it's a hold button (like a 'Ctrl' key held)
        // _inputActions.Player.Crouch.performed += ctx => OnCrouchInput?.Invoke(true);
        // _inputActions.Player.Crouch.canceled += ctx => OnCrouchInput?.Invoke(false);


        // For Run, assuming it's a hold button (like 'Shift')
        _inputActions.Player.Run.performed += ctx => OnRunInput?.Invoke(true);
        _inputActions.Player.Run.canceled += ctx => OnRunInput?.Invoke(false);

        _inputActions.Player.Look.performed += ctx => OnLookInput?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.Player.Look.canceled += ctx => OnLookInput?.Invoke(Vector2.zero);

        // --- Other Actions (Uncomment and implement when ready) ---
        // _inputActions.Player.PrimaryAttack.performed += ctx => OnPrimaryAttackInput?.Invoke();
        // _inputActions.Player.SecondaryAttack.performed += ctx => OnSecondaryAttackInput?.Invoke();
        // _inputActions.Player.Skill1.performed += ctx => OnSkill1Input?.Invoke();
        // _inputActions.Player.DodgeRoll.performed += ctx => OnDodgeRollInput?.Invoke();
        // _inputActions.Player.Interact.performed += ctx => OnInteractInput?.Invoke();
        // _inputActions.Player.Pause.performed += ctx => OnPauseInput?.Invoke(); // For menu
    }
}
