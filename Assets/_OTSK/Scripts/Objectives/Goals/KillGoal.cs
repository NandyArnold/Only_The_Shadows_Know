// KillGoal.cs

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KillGoal : ObjectiveGoal
{
    public KillGoal(KillGoal dataFromSO) : base(dataFromSO) // <-- Call the base constructor
    {
        // Only copy the fields unique to this class
        this.requiredEnemyIDs = new List<string>(dataFromSO.requiredEnemyIDs);
        this.onEnemyDiedEventChannel = dataFromSO.onEnemyDiedEventChannel;
    }

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
             UpdateUI();
            Evaluate();
        }
    }

    public override bool IsAlreadyCompleted()
    {
        // If there are no specific enemies required, the goal can't be pre-completed.
        if (requiredEnemyIDs == null || requiredEnemyIDs.Count == 0) return false;

        // Check the tracker for the count of the first enemy type required.
        // NOTE: This assumes simple goals like "Kill 5 Goblins". For complex goals
        // like "Kill 5 Goblins AND 2 Trolls", this logic would need to be expanded.
        int alreadyKilledCount = AccomplishmentTracker.Instance.GetKillCount(requiredEnemyIDs[0]);
        return alreadyKilledCount >= requiredAmount;
    }

}