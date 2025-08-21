// WeaponSO.cs - NEW UNIFIED VERSION
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceImpact
{
    public SurfaceType surface;
    public SoundDefinition sound;
}
public abstract class WeaponSO : ScriptableObject, IWeapon
{

    [Header("Targeting Settings")] 
    [SerializeField] public LayerMask lineOfSightBlockingLayers;
    public LayerMask LineOfSightBlockingLayers => lineOfSightBlockingLayers;

    [Header("Weapon Info")]
    public string weaponName;
    public bool usesAimIK;
    

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

    [Header("UI Info")]
    public string weaponDescription;
    public string primaryAttackName;
    [TextArea] public string primaryAttackDescription;
    public string secondaryAttackName;
    [TextArea] public string secondaryAttackDescription;
    public string tertiaryAttackName;
    [TextArea] public string tertiaryAttackDescription;
    public Sprite weaponIcon;

    [Header("Audio")]
    public SoundDefinition equipSound;
    public SoundDefinition unequipSound;
    [Space]
    public SoundDefinition defaultImpactSound; // Fallback for unknown surfaces
    public List<SurfaceImpact> impactSounds;


    // Damage profiles are defined in the specific weapon scripts (DaggerSO, BowSO, etc.)
    public abstract void PrimaryAttack(PlayerCombat combatController);
    public abstract void SecondaryAttack(PlayerCombat combatController);

    public SoundDefinition GetImpactSound(SurfaceType surfaceType)
    {
        foreach (var impact in impactSounds)
        {
            if (impact.surface == surfaceType)
            {
                return impact.sound;
            }
        }
        // If no specific sound is found, return the default
        return defaultImpactSound;
    }

}