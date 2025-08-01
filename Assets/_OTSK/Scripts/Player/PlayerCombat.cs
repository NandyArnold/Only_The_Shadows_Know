// PlayerCombat.cs - REFACTORED

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

    #region Standard Methods
    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();

        _daggerAnimation = GetComponent<DaggerAnimation>();

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
    }

    // The Handle methods are now very simple one-liners.
    private void HandlePrimaryAttack()
    {
        _currentWeapon?.PrimaryAttack(this);
    }

    private void HandleSecondaryAttack()
    {
        _currentWeapon?.SecondaryAttack(this);
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
}