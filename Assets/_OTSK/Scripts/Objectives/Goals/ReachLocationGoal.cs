// ReachLocationGoal.cs (CORRECTED)

using System;
using UnityEngine;

[Serializable]
public class ReachLocationGoal : ObjectiveGoal
{
    public ReachLocationGoal(ReachLocationGoal dataFromSO) : base(dataFromSO) // <-- Call the base constructor
    {
        // Now we only need to copy fields unique to this class
        this.requiredTriggerID = dataFromSO.requiredTriggerID;
        this.onTriggerEnteredEventChannel = dataFromSO.onTriggerEnteredEventChannel;
    }

    [Header("Goal Settings")]
    [Tooltip("The ID of the ObjectiveTriggerVolume in the scene that completes this goal.")]
    public string requiredTriggerID;

    [Header("Event Channel")]
    [Tooltip("The event channel to listen to for trigger volume entries.")]
    public StringGameEvent onTriggerEnteredEventChannel;

    protected override void SubscribeToEvents()
    {
        if (onTriggerEnteredEventChannel != null)
        {
            // --- ADD THIS LOG ---
            Debug.Log($"<color=yellow>[ReachLocationGoal]</color> Subscribing to event channel '{onTriggerEnteredEventChannel.name}'.");
            // --------------------
            onTriggerEnteredEventChannel.OnEventRaised += HandleTriggerEntered;
        }
        else
        {
            // --- ADD THIS LOG ---
            Debug.LogWarning("[ReachLocationGoal] Attempted to subscribe, but the Event Channel is NULL!");
            // --------------------
        }
    }

    protected override void UnsubscribeFromEvents()
    {
        if (onTriggerEnteredEventChannel != null)
        {
            onTriggerEnteredEventChannel.OnEventRaised -= HandleTriggerEntered;
        }
    }

    private void HandleTriggerEntered(string triggerID)
    {
        // --- DEBUG LOG #1 ---
        // This tells us if the goal is hearing the event at all.
        Debug.Log($"[ReachLocationGoal] Heard an event with trigger ID: '{triggerID}'. I am looking for ID: '{requiredTriggerID}'.");

        // Check if the trigger that was activated is the one we're waiting for.
        if (triggerID == requiredTriggerID)
        {
            // --- DEBUG LOG #2 ---
            // This tells us if the IDs matched and if we are proceeding.
            Debug.Log("[ReachLocationGoal] IDs MATCH! Completing goal.");

            currentAmount++;
            UpdateUI();
            Evaluate();
        }
    }

}