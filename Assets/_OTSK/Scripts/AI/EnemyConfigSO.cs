using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig_", menuName = "Only The Shadows Know/Enemy Configuration")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("Enemy Info")]
    public EnemyType enemyType;
    public string displayName;

    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float detectionRange = 15f;
    [Range(0, 360)]
    public float detectionConeAngle = 90f;
    public float hearingThreshold = 50f;
}