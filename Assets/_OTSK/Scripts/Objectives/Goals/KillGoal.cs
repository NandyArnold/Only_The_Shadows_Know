// KillGoal.cs

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KillGoal : ObjectiveGoal
{
    [Header("Goal Settings")]
    [Tooltip("The unique IDs of the enemy types that count towards this goal.")]
    public List<string> requiredEnemyIDs;

    [Header("Event Channel")]
    [Tooltip("The event channel that fires when any enemy dies.")]
    public StringGameEvent onEnemyDiedEventChannel;


    protected override void SubscribeToEvents()
    {
        if (onEnemyDiedEventChannel != null)
        {
            onEnemyDiedEventChannel.OnEventRaised += HandleEnemyDied;
        }
        else
        {
            Debug.LogWarning("[KillGoal] Attempted to subscribe, but the Event Channel is NULL!");
        }
    }

    protected override void UnsubscribeFromEvents()
    {
        if (onEnemyDiedEventChannel != null)
        {
            onEnemyDiedEventChannel.OnEventRaised -= HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(string enemyID)
    {
        Debug.Log($"[KillGoal] Heard an enemy death event with ID: '{enemyID}'. Checking against my list...");
        if (requiredEnemyIDs.Contains(enemyID))
        {
            Debug.Log($"[KillGoal] ID '{enemyID}' MATCHED! Updating progress.");
            currentAmount++;
            Evaluate();
        }
    }
}