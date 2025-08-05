using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyResistances : MonoBehaviour
{
    [SerializeField] private List<ResistanceEntry> resistances;

    public float GetMultiplier(DamageTypeSO damageType)
    {
        var entry = resistances.FirstOrDefault(r => r.damageType == damageType);
        // If we find a specific resistance entry, return its multiplier.
        // Otherwise, return 1 for normal damage.
        return entry != null ? entry.multiplier : 1f;
    }
}

