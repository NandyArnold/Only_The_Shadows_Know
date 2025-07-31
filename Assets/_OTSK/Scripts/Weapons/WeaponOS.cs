// WeaponSO.cs

using UnityEngine;

/// <summary>
/// An abstract base class for all weapon ScriptableObjects.
/// </summary>
public abstract class WeaponSO : ScriptableObject, IWeapon
{
    [Header("Weapon Info")]
    public string weaponName;
    [TextArea]
    public string weaponDescription;

    // Abstract methods that must be implemented by child classes (Dagger, Bow, etc.)
    public abstract void PrimaryAttack(PlayerCombat combatController);
    public abstract void SecondaryAttack(PlayerCombat combatController);
}
