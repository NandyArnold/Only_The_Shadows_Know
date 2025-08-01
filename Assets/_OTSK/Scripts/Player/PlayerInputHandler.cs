// PlayerInputHandler.cs - UPGRADED

using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    // --- Public Events for other scripts to subscribe to ---
    public event Action<Vector2> OnMoveInput;
    public event Action OnJumpInput;
    public event Action<bool> OnCrouchInput;
    public event Action<bool> OnRunInput;

    public event Action OnPrimaryAttackInput;
    public event Action OnSecondaryAttackInput;     // For simple presses (Dagger)
    public event Action OnSecondaryAttackPressed;   // For starting a hold (Bow)
    public event Action OnSecondaryAttackReleased;  // For ending a hold (Bow)

    public event Action OnWeapon1Input;
    public event Action OnWeapon2Input;
    public event Action OnWeapon3Input;

    public event Action<Vector2> OnLookInput; // For CameraController
    public event Action<int> OnSkillInput;
    public event Action OnDodgeRollInput;

    private PlayerInputActions _inputActions;
    private InputActionMap _playerMap;
    private InputActionMap _uiMap;
    private bool _isCrouchToggleActive = false;

    private float _lastDodgeTapTime;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();

        // Get references to the maps
        _playerMap = _inputActions.asset.FindActionMap("Player");
        _uiMap = _inputActions.asset.FindActionMap("UI");

        SetupInputCallbacks();
    }

    private void Start()
    {
        // Subscribe to the CursorManager's event to know when to switch maps.
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnCursorLockStateChanged += HandleCursorLockStateChanged;
            // Set the initial state
            HandleCursorLockStateChanged(true); // Start with cursor locked
        }
    }

    private void OnDestroy()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnCursorLockStateChanged -= HandleCursorLockStateChanged;
        }
    }

    private void HandleCursorLockStateChanged(bool isLocked)
    {
        if (isLocked)
        {
            SwitchActionMap("Player");
        }
        else
        {
            SwitchActionMap("UI");
        }
    }

    private void SetupInputCallbacks()
    {
        // --- Player Map Actions ---
        _inputActions.Player.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.Player.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero);
        _inputActions.Player.Jump.performed += ctx => OnJumpInput?.Invoke();
        _inputActions.Player.Crouch.performed += ctx => {
            _isCrouchToggleActive = !_isCrouchToggleActive;
            OnCrouchInput?.Invoke(_isCrouchToggleActive);
        };
        _inputActions.Player.Run.performed += ctx => OnRunInput?.Invoke(true);
        _inputActions.Player.Run.canceled += ctx => OnRunInput?.Invoke(false);

        _inputActions.Player.PrimaryAttack.performed += ctx => OnPrimaryAttackInput?.Invoke();
        _inputActions.Player.SecondaryAttack.performed += ctx =>
        {
            // When the button is pressed, fire BOTH the simple and the pressed events
            OnSecondaryAttackInput?.Invoke();
            OnSecondaryAttackPressed?.Invoke();
        };
        _inputActions.Player.SecondaryAttack.canceled += ctx => OnSecondaryAttackReleased?.Invoke();




        _inputActions.Player.Weapon1.performed += ctx => OnWeapon1Input?.Invoke();
        _inputActions.Player.Weapon2.performed += ctx => OnWeapon2Input?.Invoke();
        _inputActions.Player.Weapon3.performed += ctx => OnWeapon3Input?.Invoke();

        _inputActions.Player.Look.performed += ctx => OnLookInput?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.Player.Look.canceled += ctx => OnLookInput?.Invoke(Vector2.zero);

        // --- Toggle Input (Both Maps) ---
        _inputActions.Player.ToggleCursorMode.performed += HandleToggleInput;
        _inputActions.UI.ToggleCursorMode.performed += HandleToggleInput;

        _inputActions.Player.Skill1.performed += ctx => OnSkillInput?.Invoke(0);
        _inputActions.Player.Skill2.performed += ctx => OnSkillInput?.Invoke(1);
        _inputActions.Player.Skill3.performed += ctx => OnSkillInput?.Invoke(2);

        _inputActions.Player.DodgeRoll.performed += ctx => HandleDodgeInput();
    }

    private void HandleToggleInput(InputAction.CallbackContext context)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ToggleCursorMode();
        }
    }

    public void SwitchActionMap(string mapName)
    {
        // Disable all maps first
        _playerMap.Disable();
        _uiMap.Disable();
        // We will add the Targeting map here later

        // Enable the requested map
        switch (mapName)
        {
            case "Player":
                _playerMap.Enable();
                break;
            case "UI":
                _uiMap.Enable();
                break;
        }
    }
    private void HandleDodgeInput()
    {
        // Check if the time since the last tap is within our window.
        if (Time.time - _lastDodgeTapTime < doubleTapTimeWindow)
        {
            // It's a double tap! Fire the event.
            OnDodgeRollInput?.Invoke();
        }

        // Always update the time of the last tap.
        _lastDodgeTapTime = Time.time;
    }
}