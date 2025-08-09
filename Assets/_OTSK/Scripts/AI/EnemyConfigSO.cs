// EnemyConfigSO.cs - UPGRADED

using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig_", menuName = "Only The Shadows Know/Enemy Configuration")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Enemy Info")]
    public EnemyType enemyType;
    public string displayName;

    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float attackAnimationDuration = 1.2f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float alertSpeed = 3.5f;
    public float walkCombatSpeed = 1.5f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float visionRange = 15f; // Renamed from detectionRange

    [Range(0, 360)]
    public float detectionConeAngle = 90f;
    public float hearingRange = 20f;
    public float hearingThreshold = 50f;
    public float combatEntryRange = 10f;
    public float combatLockRange = 30f;

    [Header("Combat Logic")]
    [Tooltip("If the enemy can't attack the player for this long while in combat, it sounds an alarm.")]
    public float cantAttackAlarmTime = 10f; 
    [Tooltip("Depending on in game events, does this enemy instantly know where the player is?")]
    public bool instantlyKnowsAttackerLocation = true;
    [Tooltip("When in combat, the distance from which it walks instead of running.")]
    public float walkCombatRange = 7f;


}