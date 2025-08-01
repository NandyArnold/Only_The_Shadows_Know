// PlayerCombat.cs - REFACTORED
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Public properties for other scripts (like WeaponSO) to access.
    public PlayerAnimationController PlayerAnimationController => playerAnimationController;
    public Transform HandSocketR => handSocketR;
    public Transform HandSocketL => handSocketL;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager;

    [Header("Socket Transforms")]
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    private WeaponSO _currentWeapon;
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;

    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;
    private bool _isAiming = false;
    private bool _isFocused = false;

    #region Standard Methods
    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();

        _daggerAnimation = GetComponent<DaggerAnimation>();
        _bowAnimation = GetComponent<BowAnimation>();

        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            SwitchWeapon(availableWeapons[0]);
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed += HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased += HandleSecondaryAttackRelease;


            playerInputHandler.OnWeapon1Input += () => HandleWeaponSwitch(0);
            playerInputHandler.OnWeapon2Input += () => HandleWeaponSwitch(1);
            playerInputHandler.OnWeapon3Input += () => HandleWeaponSwitch(2);
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput -= HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput -= HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed -= HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased -= HandleSecondaryAttackRelease;


            playerInputHandler.OnWeapon1Input -= () => HandleWeaponSwitch(0);
            playerInputHandler.OnWeapon2Input -= () => HandleWeaponSwitch(1);
            playerInputHandler.OnWeapon3Input -= () => HandleWeaponSwitch(2);
        }
    }
    #endregion

    public void SwitchWeapon(WeaponSO newWeapon)
    {
        _currentWeapon = newWeapon;
        weaponManager.EquipNewWeapon(newWeapon);

        if (newWeapon is DaggerSO)
        {
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
        }
        else if (newWeapon is BowSO)
        {
            playerAnimationController.SetWeaponAnimation(_bowAnimation);
        }
        Debug.Log($"Equipped: {_currentWeapon.weaponName}");
    }

    // The Handle methods are now very simple one-liners.
    private void HandlePrimaryAttack()
    {
        if (_currentWeapon == null) return;

        // If we are aiming with a bow, fire a focused shot. Otherwise, normal primary attack.
        if (_isAiming && _currentWeapon is BowSO)
        {
            // We'll create a special method in BowSO for this later
            Debug.Log("Firing a FOCUSED shot.");
        }
        else
        {
            _currentWeapon.PrimaryAttack(this);
        }
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon == null) return;

        // If we have a bow, toggle aim. Otherwise, do a normal secondary attack.
        // Guard Clause: If we're holding a bow, this event is ignored.
        if (_currentWeapon is BowSO) return;

        // For any other weapon, perform the secondary attack.
        _currentWeapon?.SecondaryAttack(this);
    }
    private void HandleSecondaryAttackPress()
    {
        // Only do something if we have a bow equipped and are in the aim stance
        if (_currentWeapon is BowSO && _isAiming)
        {
            SetFocusState(true);
        }
    }

    // This handler is for RELEASING A HOLD (for the Bow)
    private void HandleSecondaryAttackRelease()
    {
        if (_currentWeapon is BowSO && _isAiming)
        {
            SetFocusState(false);
        }
    }

    private void HandleWeaponSwitch(int weaponIndex)
    {
        // Check if the requested weapon exists in our list
        if (availableWeapons != null && weaponIndex < availableWeapons.Count && availableWeapons[weaponIndex] != null)
        {
            // Check if we aren't already holding that weapon
            if (_currentWeapon != availableWeapons[weaponIndex])
            {
                SwitchWeapon(availableWeapons[weaponIndex]);
            }
        }
    }
    private void ToggleAimMode()
    {
        _isAiming = !_isAiming;
        OnAimStateChanged?.Invoke(_isAiming); // Broadcast the change
        playerAnimationController.SetAimingState(_isAiming);
        Debug.Log($"Aiming: {_isAiming}");
    }
    private void SetFocusState(bool isFocused)
    {
        // Guard Clause: Only run if the state is actually changing.
        if (_isFocused == isFocused) return;

        _isFocused = isFocused;
        // Notify any listening systems (like the CameraController) of the change.
        OnFocusStateChanged?.Invoke(_isFocused);
        Debug.Log($"Focus state changed to: {_isFocused}");
    }
}