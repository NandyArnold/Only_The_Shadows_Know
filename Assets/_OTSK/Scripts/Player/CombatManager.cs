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

    private void Update()
    {
        // When you press the 'I' key...
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // ...check if the combat list is supposed to be empty.
            if (_enemiesInCombat.Count > 0)
            {
                Debug.Log($"<color=orange>COMBAT MANAGER DEBUG: There are {_enemiesInCombat.Count} enemies still registered in combat.</color>");
                foreach (var enemy in _enemiesInCombat)
                {
                    // Print the name and status of each enemy in the list.
                    // This will expose the "ghost" enemy that is dead but still registered.
                    string configName = enemy.Config != null ? enemy.Config.name : "CONFIG NOT FOUND";
                    string status = (enemy.GetComponent<EnemyHealth>().IsDead) ? " (Is Dead)" : " (Is Alive)";

                    Debug.Log($"--> Config: {configName} on GameObject: {enemy.name}{status}");
                }
            }
            else
            {
                Debug.Log("<color=green>COMBAT MANAGER DEBUG: The combat list is empty. All enemies correctly unregistered.</color>");
            }
        }
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