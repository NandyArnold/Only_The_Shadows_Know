// CombatManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public bool IsPlayerInCombat { get; private set; } = false;

    public event Action OnCombatStart;
    public event Action OnCombatEnd;

    private readonly List<Enemy> _enemiesInCombat = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // UPDATED: Now takes the enemy that started the fight.
    public void RegisterEnemyInCombat(Enemy enemy)
    {
        if (!_enemiesInCombat.Contains(enemy))
        {
            _enemiesInCombat.Add(enemy);
        }

        // If this is the FIRST enemy to enter combat, start the combat state.
        if (!IsPlayerInCombat)
        {
            IsPlayerInCombat = true;
            OnCombatStart?.Invoke();
            Debug.Log("<color=red>COMBAT STARTED</color>");
        }
    }

    // NEW: Called by an enemy when it is defeated.
    public void UnregisterEnemyFromCombat(Enemy enemy)
    {
        if (_enemiesInCombat.Contains(enemy))
        {
            _enemiesInCombat.Remove(enemy);
        }

        // If that was the LAST enemy, end the combat state.
        if (_enemiesInCombat.Count == 0 && IsPlayerInCombat)
        {
            IsPlayerInCombat = false;
            OnCombatEnd?.Invoke();
            Debug.Log("<color=green>COMBAT ENDED</color>");
        }
    }
}