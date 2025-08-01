// AnimancySO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Animancy", menuName = "Only The Shadows Know/Weapons/Animancy")]
public class AnimancySO : WeaponSO
{
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        Debug.Log("Used ranged Soul Sever.");
    }

    public override void SecondaryAttack(PlayerCombat combatController)
    {
        Debug.Log("Used melee Soul Sever.");
    }
}