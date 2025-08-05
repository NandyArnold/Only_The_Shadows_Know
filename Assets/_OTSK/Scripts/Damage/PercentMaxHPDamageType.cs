using UnityEngine;

[CreateAssetMenu(fileName = "DT_PercentMaxHP", menuName = "Only The Shadows Know/Combat/Damage Types/Percent Max HP")]
public class PercentMaxHPDamageType : DamageTypeSO
{
    // Percent damage ignores the baseValue and calculates from the target's health.
    public override float CalculateDamage(float baseValue, GameObject target)
    {
        if (target.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            // baseValue here is treated as the percentage (e.g., 1.0 = 100%)
            return enemyHealth.MaxHealth * baseValue;
        }
        return 0; // Or handle for player health if needed
    }
}