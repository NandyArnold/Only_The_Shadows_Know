// PlayerCombat.cs - REFACTORED
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager; // New reference

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    private WeaponSO _currentWeapon;
    private DaggerAnimation _daggerAnimation;

    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>(); // New

        _daggerAnimation = GetComponent<DaggerAnimation>();

        // Equip the first weapon by default.
        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            EquipWeapon(availableWeapons[0]);
        }
    }

    // This method is now much simpler.
    public void EquipWeapon(WeaponSO newWeapon)
    {
        _currentWeapon = newWeapon;

        // 1. Tell the WeaponManager to handle the visuals.
        weaponManager.OnWeaponEquipped(newWeapon);

        // 2. Tell the PlayerAnimationController to handle the animations.
        if (newWeapon is DaggerSO)
        {
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
        }

        Debug.Log($"Equipped: {_currentWeapon.weaponName}");
    }

    #region Unchanged Methods
    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput -= HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput -= HandleSecondaryAttack;
        }
    }
    private void HandlePrimaryAttack()
    {
        if (_currentWeapon == null) return;
        playerAnimationController.TriggerPrimaryAttack();
        _currentWeapon.PrimaryAttack(this);
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon == null) return;
        playerAnimationController.TriggerSecondaryAttack();
        _currentWeapon.SecondaryAttack(this);
    }
    #endregion
}