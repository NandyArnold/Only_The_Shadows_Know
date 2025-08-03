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

    [Header("Combat Points")]
    [SerializeField] private Transform firePoint;
    public Transform FirePoint => firePoint;
    public bool IsFocused => _isFocused;

    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;

    private WeaponSO _currentWeapon;
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;

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


            playerInputHandler.OnWeapon1Input += HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input += HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input += HandleWeapon3Switch;
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


            playerInputHandler.OnWeapon1Input -= HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input -= HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input -= HandleWeapon3Switch;
        }
    }
    #endregion

    public void SwitchWeaponByIndex(int index)
    {
        if (index < 0 || index >= availableWeapons.Count) return;
        SwitchWeapon(availableWeapons[index]);
    }

    public void SwitchWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null || newWeapon == _currentWeapon) return;
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

        SetAimState(newWeapon is BowSO);

        if (newWeapon is DaggerSO)
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
        else if (newWeapon is BowSO)
            playerAnimationController.SetWeaponAnimation(_bowAnimation);
    }

    private void SetAimState(bool isAiming)
    {
        if (_isAiming == isAiming) return;
        _isAiming = isAiming;
        OnAimStateChanged?.Invoke(_isAiming);
        playerAnimationController.SetAimingState(_isAiming);
        if (!_isAiming && _isFocused) SetFocusState(false);
    }

    private void HandlePrimaryAttack()
    {
        if (_currentWeapon is BowSO && _isFocused)
        {
            _currentWeapon.SecondaryAttack(this);
        }
        else
        {
            // Otherwise, it's a normal primary attack (dagger slash, or bow hip-fire).
            _currentWeapon?.PrimaryAttack(this);
        }
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon is BowSO) return;
        _currentWeapon?.SecondaryAttack(this);
    }
    private void HandleSecondaryAttackPress()
    {
        if (_currentWeapon is BowSO && _isAiming) SetFocusState(true);
    }

    // This handler is for RELEASING A HOLD (for the Bow)
    private void HandleSecondaryAttackRelease()
    {
        if (_currentWeapon is BowSO && _isAiming) SetFocusState(false);
    }

    private void HandleWeapon1Switch() => SwitchWeapon(availableWeapons[0]);
         private void HandleWeapon2Switch() => SwitchWeapon(availableWeapons[1]);
    private void HandleWeapon3Switch() => SwitchWeapon(availableWeapons[2]);
    //private void HandleWeaponSwitch(int weaponIndex)
    //{
    //    // Check if the requested weapon exists in our list
    //    if (availableWeapons != null && weaponIndex < availableWeapons.Count && availableWeapons[weaponIndex] != null)
    //    {
    //        // Check if we aren't already holding that weapon
    //        if (_currentWeapon != availableWeapons[weaponIndex])
    //        {
    //            SwitchWeapon(availableWeapons[weaponIndex]);
    //        }
    //    }
    //}
    //private void ToggleAimMode()
    //{
    //    _isAiming = !_isAiming;
    //    OnAimStateChanged?.Invoke(_isAiming); // Broadcast the change
    //    playerAnimationController.SetAimingState(_isAiming);
    //    Debug.Log($"Aiming: {_isAiming}");
    //}
    private void SetFocusState(bool isFocused)
    {
        if (_isFocused == isFocused) return;
        _isFocused = isFocused;
        OnFocusStateChanged?.Invoke(_isFocused);
    }
}