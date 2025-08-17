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
    public event Action<Enemy> OnEnemyDiedInCombat;

    private readonly List<Enemy> _enemiesInCombat = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // UPDATED: Now takes the enemy that started the fight.
    public void RegisterEnemyInCombat(Enemy enemy)
    {
        //// --- STACK TRACE DEBUG TOOL ---
        //var stackTrace = new System.Diagnostics.StackTrace();
        //string callChain = "CALL STACK:\n";
        //// We loop up to 5 frames, or as many as exist in the stack.
        //// We start at i = 1 to skip the current method itself.
        //for (int i = 1; i < stackTrace.FrameCount && i < 6; i++)
        //{
        //    var frame = stackTrace.GetFrame(i);
        //    var method = frame.GetMethod();
        //    // Add an arrow and the "ClassName.MethodName" to our string.
        //    callChain += $"--> {method.DeclaringType.Name}.{method.Name}\n";
        //}
        //Debug.Log(callChain);
        //// --- END DEBUG TOOL ---


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

    //  Called by an enemy when it is defeated.
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

    public void ReportEnemyDeath(Enemy deadEnemy, bool isSilentKill)
    {
        Debug.Log($"<color=cyan>[CombatManager]</color> ReportEnemyDeath() called. isSilentKill is: {isSilentKill}");
        // First, unregister the enemy from the combat list.
        UnregisterEnemyFromCombat(deadEnemy);

        // Then, announce the death to any listeners (like other AIs).
        if (!isSilentKill)
        {
            Debug.Log("<color=green>[CombatManager]</color> Kill was NOT silent. Firing OnEnemyDiedInCombat event.");
            OnEnemyDiedInCombat?.Invoke(deadEnemy);
        }
    }
}