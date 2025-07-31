// PlayerCombat.cs - UPGRADED

using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    private WeaponSO _currentWeapon;

    // --- Weapon Animation Components ---
    // We get references to all possible weapon animation components on the player.
    private DaggerAnimation _daggerAnimation;
    // private BowAnimation _bowAnimation; // We'll add this later

    private void Awake()
    {
        // Get component references
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerAnimationController == null) playerAnimationController = GetComponent<PlayerAnimationController>();

        // Get weapon animation component references
        _daggerAnimation = GetComponent<DaggerAnimation>();

        // Equip the first weapon by default.
        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            EquipWeapon(availableWeapons[0]);
        }
    }

    // ... (OnEnable/OnDisable are unchanged) ...
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

        // Tell the animator to fire the attack, which will use the correct weapon animation.
        playerAnimationController.TriggerPrimaryAttack();

        // The weapon's logic (damage, etc.) is still handled by the SO.
        _currentWeapon.PrimaryAttack(this);
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon == null) return;
        playerAnimationController.TriggerSecondaryAttack();
        _currentWeapon.SecondaryAttack(this);
    }

    public void EquipWeapon(WeaponSO newWeapon)
    {
        _currentWeapon = newWeapon;

        // This is the key part: Tell the PlayerAnimationController which animation component to use.
        if (newWeapon is DaggerSO)
        {
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
        }
        // else if (newWeapon is BowSO)
        // {
        //     playerAnimationController.SetWeaponAnimation(_bowAnimation);
        // }

        Debug.Log($"Equipped: {_currentWeapon.weaponName}");
    }
}