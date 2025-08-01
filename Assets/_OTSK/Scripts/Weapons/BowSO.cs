// BowSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Bow", menuName = "Only The Shadows Know/Weapons/Bow")]
public class BowSO : WeaponSO
{
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        Debug.Log("Fired an unfocused arrow.");
    }

    public override void SecondaryAttack(PlayerCombat combatController)
    {
        Debug.Log("Toggled Aim Mode.");
    }
}