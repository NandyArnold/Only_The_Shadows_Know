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

    public void OnGoalCompleted(ObjectiveGoal completedGoal)
    {
        Debug.Log($"Goal completed for objective: {SourceSO.objectiveDescription}");
        //State = ObjectiveState.Completed;
        // This will notify the ObjectiveManager.
        ObjectiveManager.Instance?.CompleteObjective(this);
    }

   
    private ObjectiveGoal CreateGoalInstance(ObjectiveSO so)
    {
        switch (so.goalType)
        {
            case ObjectiveGoalType.Kill:
                var killGoalData = so.killGoal;
                return new KillGoal(so.killGoal);
               

            case ObjectiveGoalType.ReachLocation:
                var reachData = so.reachLocationGoal;
                return new ReachLocationGoal(so.reachLocationGoal);
              

            case ObjectiveGoalType.Destroy:
                var destroyData = so.destroyGoal;
                return new DestroyGoal(so.destroyGoal);
              
        }

        Debug.LogError($"[ObjectiveInstance] Could not create goal instance for Objective '{so.objectiveID}'. The Goal Type '{so.goalType}' is not handled in the CreateGoalInstance method.", so);
        return null;
    }

   
    public void Start()
    {
        Debug.Log($"<color=lime>[ObjectiveInstance]</color> Starting objective '{SourceSO.objectiveDescription}'. Initializing its goal.");
        State = ObjectiveState.Active;
        Goal?.Initialize(this); // The ?. ensures this only runs if Goal is not null.

        //Debug.Log($"<color=cyan>[ObjectiveInstance]</color> About to broadcast initial UI state for '{SourceSO.objectiveDescription}'.");
        //Goal?.UpdateUI();
        //Debug.Log($"<color=cyan>[ObjectiveInstance]</color> Broadcast complete.");
    }

    public void MarkCompletedFromLoad()
    {
        State = ObjectiveState.Completed;
    }

    public void NotifyUIOfInitialState()
    {
        Debug.Log($"<color=cyan>[ObjectiveInstance]</color> About to broadcast initial UI state for '{SourceSO.objectiveDescription}'.");
        Goal?.UpdateUI();
        Debug.Log($"<color=cyan>[ObjectiveInstance]</color> Broadcast complete.");
    }


    public void CleanUp()
    {
        Goal?.CleanUp();
    }
}