using UnityEngine;

[CreateAssetMenu(fileName = "DT_Flat", menuName = "Only The Shadows Know/Combat/Damage Types/Flat")]
public class FlatDamageType : DamageTypeSO
{
    // Flat damage is simple: it just returns the base value.
    public override float CalculateDamage(float baseValue, GameObject target)
    {
        return baseValue;
    }
}