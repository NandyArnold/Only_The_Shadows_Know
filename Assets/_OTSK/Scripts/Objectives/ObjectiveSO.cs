using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Objective_", menuName = "Only The Shadows Know/Objectives/Single Objective")]
public class ObjectiveSO : ScriptableObject
{
    [Tooltip("Unique ID for this objective within its chain.")]
    public string objectiveID;

    [Tooltip("The display name of the quest. (For UI)")] 
    public string objectiveTitle;

    [TextArea]
    [Tooltip("Text displayed to the player in the HUD (e.g., 'Find the Hidden Artifact').")]
    public string objectiveDescription;

    [Header("Discovery")]
    [Tooltip("If true, this objective will not be shown in the UI until it is completed.")]
    public bool isHidden = false;

    [Tooltip("If true, this objective will persist between level changes.")] 
    public bool isPersistent = false;

    [Header("Goal")]
    public ObjectiveGoalType goalType;

    [Header("Rewards")]
    [Tooltip("A list of rewards to be granted when this objective is completed.")]
    [SerializeReference] public List<RewardAction> rewards = new List<RewardAction>();

    // We add a field for each goal type. The custom editor will only show the relevant one.
    public KillGoal killGoal;
    public ReachLocationGoal reachLocationGoal;
    public DestroyGoal destroyGoal;

    [Tooltip("The ID of the in-world trigger that will complete this objective.")]
    public string completionTriggerID;

    [Tooltip("The ID of the GameObject in the scene that serves as the target location for this objective.")]
    public string targetLocationID;

    public ObjectiveType objectiveType = ObjectiveType.MainObjective;

   
    public ObjectiveGoal GetGoal()
    {
        switch (goalType)
        {
            case ObjectiveGoalType.Kill:
                return killGoal;
            case ObjectiveGoalType.ReachLocation:
                return reachLocationGoal;
            case ObjectiveGoalType.Destroy: 
                return destroyGoal;
                // Add cases for future goal types here
        }
        return null;
    }
}
