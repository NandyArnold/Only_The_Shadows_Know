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
    public event Action OnTertiaryAttackInput;    // For Animancy or other special attacks

    public event Action OnToggleInvulnerabilityInput;

    public event Action OnWeapon1Input;
    public event Action OnWeapon2Input;
    public event Action OnWeapon3Input;

    public event Action<Vector2> OnLookInput; // For CameraController
    
    public event Action<int> OnSkillPerformedInput;
    public event Action<int> OnSkillCanceledInput;

    public event Action OnPauseInput;

    public event Action OnDodgeRollInput;

    public event Action<float> OnCycleTargetingModeInput;

    public event Action OnConfirmInput;  // For confirming Targeting Mode
    public event Action OnCancelInput;

    public event Action OnShowObjectiveInput;

    public event Action<float> OnAdjustPitchInput;

    private PlayerInputActions _inputActions;
    private InputActionMap _playerMap;
    private InputActionMap _uiMap;
    private InputActionMap _targetingMap;
    private InputActionMap _disabledMap;

    private bool _isCrouchToggleActive = false;


    private void Awake()
    {
        _inputActions = new PlayerInputActions();

        // Get references to the maps
        _playerMap = _inputActions.asset.FindActionMap("Player");
        _uiMap = _inputActions.asset.FindActionMap("UI");
        _targetingMap = _inputActions.asset.FindActionMap("Targeting");
        _disabledMap = _inputActions.asset.FindActionMap("Disabled");
 

        SetupInputCallbacks();
    }

    private void Start()
    {
        // Subscribe to the CursorManager's event to know when to switch maps.
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnStateChanged += HandleCursorStateChanged;
            // Set the initial state
            HandleCursorStateChanged(CursorState.Gameplay);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            // Set initial state
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.OnStateChanged -= HandleCursorStateChanged;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Loading)
        {
            SwitchActionMap("Disabled");
        }
    }
    private void HandleCursorStateChanged(CursorState newState)
    {
        switch (newState)
        {
            case CursorState.Gameplay:
                SwitchActionMap("Player");
                break;
            case CursorState.UI:
                SwitchActionMap("UI");
                break;
            case CursorState.Targeting:
                SwitchActionMap("Targeting");
                break;
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
        _inputActions.Player.SecondaryAttack_Press.performed += ctx => OnSecondaryAttackInput?.Invoke();

        _inputActions.Player.SecondaryAttack_Hold.performed += ctx => OnSecondaryAttackPressed?.Invoke();
        _inputActions.Player.SecondaryAttack_Hold.canceled += ctx => OnSecondaryAttackReleased?.Invoke();

        _inputActions.Player.TertiaryAttack.performed += ctx => OnTertiaryAttackInput?.Invoke();


        _inputActions.Player.Weapon1.performed += ctx => OnWeapon1Input?.Invoke();
        _inputActions.Player.Weapon2.performed += ctx => OnWeapon2Input?.Invoke();
        _inputActions.Player.Weapon3.performed += ctx => OnWeapon3Input?.Invoke();

        _inputActions.Player.Look.performed += ctx => OnLookInput?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.Player.Look.canceled += ctx => OnLookInput?.Invoke(Vector2.zero);

        // --- Toggle Input (Both Maps) ---
        _inputActions.Player.ToggleCursorMode.performed += HandleToggleInput;
        _inputActions.UI.ToggleCursorMode.performed += HandleToggleInput;

        // Callbacks for the Targeting Map
        _inputActions.Targeting.Confirm.performed += ctx => OnConfirmInput?.Invoke();
        _inputActions.Targeting.Cancel.performed += ctx => OnCancelInput?.Invoke();

        _inputActions.Targeting.AdjustPitch.performed += ctx => OnAdjustPitchInput?.Invoke(ctx.ReadValue<float>());
        _inputActions.Targeting.AdjustPitch.canceled += ctx => OnAdjustPitchInput?.Invoke(0f);



        _inputActions.Player.Skill1.performed += ctx => OnSkillPerformedInput?.Invoke(0);
        _inputActions.Player.Skill2.performed += ctx => OnSkillPerformedInput?.Invoke(1);
        _inputActions.Player.Skill3.performed += ctx => OnSkillPerformedInput?.Invoke(2);
        _inputActions.Player.Skill4.started += ctx => OnSkillPerformedInput?.Invoke(3);
        _inputActions.Player.Skill4.canceled += ctx => OnSkillCanceledInput?.Invoke(3);
        _inputActions.Player.Skill5.performed += ctx => OnSkillPerformedInput?.Invoke(4);


        _inputActions.Targeting.CycleTargetingMode.performed += ctx => OnCycleTargetingModeInput?.Invoke(ctx.ReadValue<float>());

        _inputActions.Player.DodgeRoll.performed += ctx => OnDodgeRollInput?.Invoke();

        _inputActions.Player.ShowObjective.performed += ctx => OnShowObjectiveInput?.Invoke();
        _inputActions.Player.CancelAction.performed += ctx => EventManager.Instance.CancelActionInput();

        _inputActions.Player.Pause.performed += ctx => OnPauseInput?.Invoke();
        _inputActions.UI.Unpause.performed += ctx => OnPauseInput?.Invoke();

        _inputActions.Player.Invulnerability.performed += ctx => OnToggleInvulnerabilityInput?.Invoke();
    }

    private void HandleToggleInput(InputAction.CallbackContext context)
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance?.ToggleUIMode();
        }
    }

    public void SwitchActionMap(string mapName)
    {
        // Disable all maps first
        _playerMap.Disable();
        _uiMap.Disable();
        _targetingMap.Disable();
        _disabledMap.Disable();

        // Enable the requested map
        switch (mapName)
        {
            case "Player":
                _playerMap.Enable();
                break;
            case "UI":
                _uiMap.Enable();
                break;
            case "Targeting":
                _targetingMap.Enable();
                break;
            case "Disabled": 
                _disabledMap.Enable(); 
                break;
        }
    }
  
}