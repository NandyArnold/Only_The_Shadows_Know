// Create this new script, DestructibleDataSO.cs
using System.Collections.Generic;
using UnityEngine;

// We can reuse this from our EnemyResistances script
// [System.Serializable]
// public class DamageMultiplier { ... }

[CreateAssetMenu(fileName = "DestructibleData_", menuName = "Only The Shadows Know/Destructible Data")]
public class DestructibleDataSO : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("A unique ID for this type of destructible object, used by systems like objectives.")]
    public string destructibleID;

    [Header("Stats")]
    public float maxHealth = 50f;
    public List<ResistanceEntry> resistances;

    [Header("Events")]
    [Tooltip("The event channel to raise when this object is destroyed. Used for objectives.")]
    public StringGameEvent onDestroyedEvent;
    // A helper method to get the multiplier for a specific damage type
    public float GetMultiplier(DamageTypeSO type)
    {
        foreach (var resistance in resistances)
        {
            if (resistance.damageType == type)
            {
                return resistance.multiplier;
            }
        }
        return 1f; // Default to normal damage if no specific resistance is found
    }
}