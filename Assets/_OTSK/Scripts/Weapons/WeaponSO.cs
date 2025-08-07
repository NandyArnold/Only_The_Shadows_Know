// WeaponSO.cs
using System.Collections.Generic;
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

    [Header("Sheathing & Animations")]
    public SheathSocket sheathSocket;
    public string equipTriggerName;
    public string unequipTriggerName;

    [Header("Visuals & Sockets")]
    public GameObject mainHandPrefab; // The 3D model prefab to spawn.
    public GameObject offHandWeaponPrefab; // Off-hand weapon model, if applicable.

    [Header("Combat Stats")]
    [Tooltip("Time in seconds between uses of this weapon.")]
    public float timeBetweenAttacks = 0.5f;

    public SocketType mainHandEquipSocket;   // Where it goes when equipped.
    public SocketType offHandEquipSocket; 

    public SocketType mainHandUnequipSocket; // Where it goes when unequipped.
    public SocketType offHandUnequipSocket;

    // Abstract methods that must be implemented by child classes (Dagger, Bow, etc.)


    public abstract void PrimaryAttack(PlayerCombat combatController);
    public abstract void SecondaryAttack(PlayerCombat combatController);
}
