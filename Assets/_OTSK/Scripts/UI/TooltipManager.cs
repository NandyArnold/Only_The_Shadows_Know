using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;

    [Header("Weapon UI References")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private Button lmbButton;
    [SerializeField] private Button rmbButton;
    [SerializeField] private Button mmbButton;

    private PlayerInputHandler _playerInputHandler;
    private PlayerCombat _playerCombat;

    private bool _isPanelOpen = false;

    private bool _isToggling = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;

        if (_playerInputHandler != null)
            _playerInputHandler.OnToggleDetailsInput -= TogglePanel;
    }

    private void HandlePlayerRegistered(PlayerController player)
    {
        _playerInputHandler = player.GetComponent<PlayerInputHandler>();
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnToggleDetailsInput += TogglePanel;
        }

        // NEW: Get the PlayerCombat component and subscribe to its event
        _playerCombat = player.GetComponent<PlayerCombat>();
        if (_playerCombat != null)
        {
            _playerCombat.OnWeaponSwitched += HandleWeaponSwitched;
            // Update the UI with the initially equipped weapon
            HandleWeaponSwitched(_playerCombat.CurrentWeapon);
        }
    }

    private void TogglePanel()
    {
        // If we are already in the middle of a toggle from this frame, ignore this input.
        if (_isToggling) return;

        _isPanelOpen = !_isPanelOpen;
        tooltipPanel.SetActive(_isPanelOpen);

        if (_isPanelOpen)
        {
            GameManager.Instance.UpdateGameState(GameState.Details);
        }
        else
        {
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }

        // Start the cooldown coroutine.
        StartCoroutine(ToggleCooldownRoutine());
    }

    // This coroutine creates a very short "debounce" period.
    private IEnumerator ToggleCooldownRoutine()
    {
        _isToggling = true;
        // Wait for the end of the current frame.
        yield return new WaitForEndOfFrame();
        _isToggling = false;
    }

    private void HandleWeaponSwitched(WeaponSO newWeapon)
    {
        if (newWeapon == null) return;

        // Update the main text fields
        weaponNameText.text = newWeapon.weaponName;
        weaponDescriptionText.text = newWeapon.weaponDescription;

        // Update the button labels with the attack names
        lmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.primaryAttackName;
        rmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.secondaryAttackName;

        // Show or hide the MMB button based on whether the weapon is Animancy
        bool hasTertiary = (newWeapon is AnimancySO);
        mmbButton.gameObject.SetActive(hasTertiary);
        if (hasTertiary)
        {
            mmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.tertiaryAttackName;
        }
    }
}