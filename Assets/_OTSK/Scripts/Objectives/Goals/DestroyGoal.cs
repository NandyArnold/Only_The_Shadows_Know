// DestroyGoal.cs

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DestroyGoal : ObjectiveGoal
{
    public DestroyGoal(DestroyGoal dataFromSO) : base(dataFromSO) // <-- Call the base constructor
    {
        // Only copy the fields unique to this class
        this.requiredDestructibleIDs = new List<string>(dataFromSO.requiredDestructibleIDs);
        this.onDestroyedEventChannel = dataFromSO.onDestroyedEventChannel;
    }

    [Header("Goal Settings")]
    [Tooltip("The unique IDs of the destructible objects that count towards this goal.")]
    public List<string> requiredDestructibleIDs;

    [Header("Event Channel")]
    [Tooltip("The event channel to listen to for destruction events.")]
    public StringGameEvent onDestroyedEventChannel;

    protected override void SubscribeToEvents()
    {
        if (onDestroyedEventChannel != null)
        {
            onDestroyedEventChannel.OnEventRaised += HandleDestructibleDestroyed;
        }
    }

    protected override void UnsubscribeFromEvents()
    {
        if (onDestroyedEventChannel != null)
        {
            onDestroyedEventChannel.OnEventRaised -= HandleDestructibleDestroyed;
        }
    }

    private void HandleDestructibleDestroyed(string destructibleID)
    {
        if (requiredDestructibleIDs.Contains(destructibleID))
        {
            currentAmount++;
            UpdateUI();
            Evaluate();
        }
    }
 
}