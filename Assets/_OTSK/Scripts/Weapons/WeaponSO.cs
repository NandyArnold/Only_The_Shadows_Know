// WeaponSO.cs - NEW UNIFIED VERSION
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceImpact
{
    public SurfaceType surface;
    public SoundDefinition sound = new SoundDefinition();
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
        Debug.Log($"<color=cyan>[Audio System]</color> Searching for impact sound for surface: {surfaceType}");

        foreach (var impact in impactSounds)
        {
            if (impact.surface == surfaceType)
            {
                Debug.Log($"<color=green>[Audio System]</color> Match found for {surfaceType}!");

                // --- THIS IS THE CRITICAL CHECK ---
                if (impact.sound == null || impact.sound.clips == null || impact.sound.clips.Length == 0)
                {
                    Debug.LogError($"<color=red>[Audio System]</color> The SoundDefinition for '{surfaceType}' was found, but its 'Clips' array is empty! Check your DaggerSO asset in the Inspector.", this);
                }

                return impact.sound;
            }
        }

        Debug.Log($"<color=orange>[Audio System]</color> No specific sound found for {surfaceType}. Returning default impact sound.");
        return defaultImpactSound;
    }

}