// EnemyConfigSO.cs - UPGRADED

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EnemyConfig_", menuName = "Only The Shadows Know/Enemy Configuration")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Enemy Info")]
    public EnemyType enemyType;
    public RevealableType revealableType;
    [Tooltip("Optional: A special icon prefab to display over this enemy's head when revealed.")]
    public GameObject revealIconPrefab;
    public string displayName;

    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float attackAnimationDuration = 1.2f;
    [Header("Resistances")] 
    public List<ResistanceEntry> resistances;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float alertSpeed = 3.5f;
    public float walkCombatSpeed = 1.5f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float visionRange = 15f; 
    public float deathSoundIntensity = 90f; // sound intensity when the enemy dies, used for detection


    [Header("Detection")]
    public float alertInvestigateTime = 5f;
    public float lookAroundAnimationDuration = 2.5f;

    [Header("Alarm Logic")] 
    public AlarmType alarmType = AlarmType.None;
    [Tooltip("Should this enemy sound an alarm when an ally dies?")]
    public bool soundsAlarmOnAllyDeath = false;
    [Tooltip("The Game Event to raise when this enemy successfully signals for help.")]
    public GameEvent alarmEventToRaise;
    [Tooltip("Should this enemy sound an alarm if combat lasts too long?")]
    public bool useTimeOutAlarm = false;
    [Tooltip("If the enemy is in combat for this long without winning, it raises an alarm.")]
    public float timeInCombatAlarmThreshold = 20f;
    [Tooltip("The amount of times this enemy can trigger the alarm")]
    public int maxSummonCount = 1;
    

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
    [Tooltip("The distance from which the enemy will drag other enemies in combat.")]
    public float callForHelpRadius = 15f;


}