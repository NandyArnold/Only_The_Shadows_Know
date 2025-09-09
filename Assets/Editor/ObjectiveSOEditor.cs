// ObjectiveSOEditor.cs (Place this script in an "Editor" folder)

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(ObjectiveSO))]
public class ObjectiveSOEditor : Editor
{
    private Type[] _rewardTypes;

    private void OnEnable()
    {
        // Use Reflection to find all classes in the project that are subclasses of RewardAction
        _rewardTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(RewardAction)) && !type.IsAbstract)
            .ToArray();
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
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
        EditorGUILayout.Space();
        // --- CUSTOM REWARDS LIST UI ---
        // Draw the default list UI. This will show the elements that are already there.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rewards"));

        // Add a button to bring up our custom dropdown menu.
        if (GUILayout.Button("Add Reward"))
        {
            ShowAddRewardMenu();
        }
        // -----------------------------

        // Apply any changes the user made in the Inspector
        serializedObject.ApplyModifiedProperties();
    }

    private void ShowAddRewardMenu()
    {
        var menu = new GenericMenu();
        var rewardsProperty = serializedObject.FindProperty("rewards");

        // Loop through all of our found RewardAction types
        foreach (var type in _rewardTypes)
        {
            // Add an item to the menu for each type.
            // When an item is clicked, it will call a function to add that reward type to the list.
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                // This is the action that runs when a menu item is selected
                var newReward = Activator.CreateInstance(type);
                rewardsProperty.arraySize++;
                rewardsProperty.GetArrayElementAtIndex(rewardsProperty.arraySize - 1).managedReferenceValue = newReward;
                serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }
}