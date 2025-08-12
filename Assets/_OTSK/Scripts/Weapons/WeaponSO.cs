// WeaponSO.cs - NEW UNIFIED VERSION
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponSO : ScriptableObject, IWeapon
{

    [Header("Targeting Settings")] 
    [SerializeField] public LayerMask lineOfSightBlockingLayers;
    public LayerMask LineOfSightBlockingLayers => lineOfSightBlockingLayers;

    [Header("Weapon Info")]
    public string weaponName;
    public bool usesAimIK;
    public string weaponDescription;

    [Header("Combat Stats")]
    public float timeBetweenAttacks = 0.5f;

    [Header("Visuals & Sockets")]
    public GameObject mainHandPrefab;
    public EquipSocket mainHandEquipSocket;
    public SheathSocket mainHandSheathSocket;
    [Space]
    public GameObject offHandPrefab; // Leave empty for single weapons
    public EquipSocket offHandEquipSocket;
    public SheathSocket offHandSheathSocket;

    [Header("Animation Triggers")]
    public string equipTriggerName;
    public float equipDuration = 0.5f;
    [Space]
    public string unequipTriggerName;
    public float unequipDuration = 0.5f; 
   

    // Damage profiles are defined in the specific weapon scripts (DaggerSO, BowSO, etc.)
    public abstract void PrimaryAttack(PlayerCombat combatController);
    public abstract void SecondaryAttack(PlayerCombat combatController);
}