using System.Collections.Generic;
using UnityEngine;


public class ObjectiveInstance
{
    public ObjectiveSO SourceSO { get; private set; }
    public ObjectiveGoal Goal { get; private set; }
    public ObjectiveState State { get; private set; }

    // We will expand this constructor later to create the correct goal type.
    public ObjectiveInstance(ObjectiveSO sourceSO)
    {
        this.SourceSO = sourceSO;
        this.State = ObjectiveState.Inactive;
        this.Goal = CreateGoalInstance(sourceSO);
        // TODO: Instantiate the correct ObjectiveGoal based on the SourceSO's goal type.
        // For now, we leave this blank to resolve the compile error.
    }

    /// <summary>
    /// This method is called by an ObjectiveGoal when it has been completed.
    /// </summary>
    public void OnGoalCompleted(ObjectiveGoal completedGoal)
    {
        Debug.Log($"Goal completed for objective: {SourceSO.objectiveDescription}");
        State = ObjectiveState.Completed;
        // This will notify the ObjectiveManager.
        ObjectiveManager.Instance?.CompleteObjective(this);
    }

    /// <summary>
    /// Factory method to create a new runtime instance of a goal.
    /// </summary>
    private ObjectiveGoal CreateGoalInstance(ObjectiveSO so)
    {
        switch (so.goalType)
        {
            case ObjectiveGoalType.Kill:
                var killGoalData = so.killGoal;
                return new KillGoal
                {
                    requiredAmount = killGoalData.requiredAmount,
                    requiredEnemyIDs = new System.Collections.Generic.List<string>(killGoalData.requiredEnemyIDs),
                    // --- THE FIX FOR KILLGOAL ---
                    onEnemyDiedEventChannel = killGoalData.onEnemyDiedEventChannel
                };

            case ObjectiveGoalType.ReachLocation:
                var reachData = so.reachLocationGoal;
                return new ReachLocationGoal
                {
                    requiredAmount = reachData.requiredAmount,
                    requiredTriggerID = reachData.requiredTriggerID,
                    // --- THE FIX FOR REACHLOCATIONGOAL ---
                    onTriggerEnteredEventChannel = reachData.onTriggerEnteredEventChannel
                };

            case ObjectiveGoalType.Destroy:
                var destroyData = so.destroyGoal;
                return new DestroyGoal
                {
                    requiredAmount = destroyData.requiredAmount,
                    requiredDestructibleIDs = new System.Collections.Generic.List<string>(destroyData.requiredDestructibleIDs),
                    // --- THE FIX FOR DESTROYGOAL ---
                    onDestroyedEventChannel = destroyData.onDestroyedEventChannel
                };
        }

        Debug.LogError($"[ObjectiveInstance] Could not create goal instance for Objective '{so.objectiveID}'. The Goal Type '{so.goalType}' is not handled in the CreateGoalInstance method.", so);
        return null;
    }

    /// <summary>
    /// Called to activate the objective and its goal.
    /// </summary>
    public void Start()
    {
        Debug.Log($"<color=lime>[ObjectiveInstance]</color> Starting objective '{SourceSO.objectiveDescription}'. Initializing its goal.");
        State = ObjectiveState.Active;
        Goal?.Initialize(this); // The ?. ensures this only runs if Goal is not null.
    }

    /// <summary>
    /// Called to clean up the objective, unsubscribing from events.
    /// </summary>
    public void CleanUp()
    {
        Goal?.CleanUp();
    }
}