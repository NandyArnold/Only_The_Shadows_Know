
using UnityEngine;


[CreateAssetMenu(fileName = "Objective_", menuName = "Only The Shadows Know/Objectives/Single Objective")]
public class ObjectiveSO : ScriptableObject
{
    [Tooltip("Unique ID for this objective within its chain.")]
    public string objectiveID;

    [TextArea]
    [Tooltip("Text displayed to the player in the HUD (e.g., 'Find the Hidden Artifact').")]
    public string objectiveDescription;

    [Header("Goal")]
    public ObjectiveGoalType goalType;

    // We add a field for each goal type. The custom editor will only show the relevant one.
    public KillGoal killGoal;
    public ReachLocationGoal reachLocationGoal;
    public DestroyGoal destroyGoal;

    [Tooltip("The ID of the in-world trigger that will complete this objective.")]
    public string completionTriggerID;

    [Tooltip("Location of the objective in the game world (for map markers, etc.).")]
    public Transform objectiveLocation;

    public ObjectiveType objectiveType = ObjectiveType.MainObjective;

    /// <summary>
    /// A helper method to get the active goal based on the selected goalType.
    /// </summary>
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
