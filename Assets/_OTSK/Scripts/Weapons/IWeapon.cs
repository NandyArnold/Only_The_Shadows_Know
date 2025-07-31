// IWeapon.cs

using UnityEngine;

public interface IWeapon
{
    // The method for the primary attack (e.g., Left Mouse Button).
    void PrimaryAttack(PlayerCombat combatController);

    // The method for the secondary attack (e.g., Right Mouse Button).
    void SecondaryAttack(PlayerCombat combatController);
}