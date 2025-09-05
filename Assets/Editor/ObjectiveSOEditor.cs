// ObjectiveSOEditor.cs (Place this script in an "Editor" folder)

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectiveSO))]
public class ObjectiveSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to the script we are editing
        var objectiveSO = (ObjectiveSO)target;

        // Draw the default fields we always want to see
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveDescription"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveLocation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveType"));

        EditorGUILayout.Space();

        // Draw the Goal Type enum dropdown
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goalType"));

        // --- The Magic Switch ---
        // Depending on the selected goalType, show only the relevant goal's properties.
        switch (objectiveSO.goalType)
        {
            case ObjectiveGoalType.Kill:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("killGoal"));
                break;
                case ObjectiveGoalType.ReachLocation:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reachLocationGoal"));
                break;
            case ObjectiveGoalType.Destroy: 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyGoal"));
                break;
        }

        // Apply any changes the user made in the Inspector
        serializedObject.ApplyModifiedProperties();
    }
}