using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyResistances : MonoBehaviour
{
    private readonly Dictionary<DamageTypeSO, float> _multipliers = new Dictionary<DamageTypeSO, float>();

    public void Initialize(EnemyConfigSO config)
    {
        _multipliers.Clear();
        if (config.resistances == null) return;

        foreach (var resistance in config.resistances)
        {
            if (resistance.damageType != null)
            {
                _multipliers[resistance.damageType] = resistance.multiplier;
            }
        }
    }

    public float GetMultiplier(DamageTypeSO damageType)
    {
        if (_multipliers.TryGetValue(damageType, out float multiplier))
        {
            return multiplier;
        }
        return 1f; // Default to normal damage
    }
}

