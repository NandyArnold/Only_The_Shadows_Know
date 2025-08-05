using UnityEngine;

[System.Serializable]
public class DamageMultiplier
{
    public EnemyType enemyType;
    [Min(0)]
    public float multiplier = 1f;
}
