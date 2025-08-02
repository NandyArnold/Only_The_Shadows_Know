using UnityEngine;

[CreateAssetMenu(fileName = "Objective_", menuName = "Only The Shadows Know/Objectives/Single Objective")]
public class ObjectiveSO : ScriptableObject
{
    [Tooltip("Unique ID for this objective within its chain.")]
    public string objectiveID;

    [TextArea]
    [Tooltip("Text displayed to the player in the HUD (e.g., 'Find the Hidden Artifact').")]
    public string objectiveDescription;

    [Tooltip("The ID of the in-world trigger that will complete this objective.")]
    public string completionTriggerID;
}
