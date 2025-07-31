// SceneDataSO.cs
using UnityEngine;

// Define the different types of scenes we can have.
public enum SceneType
{
    // Gameplay scenes where the cursor should be locked.
    Gameplay,
    // Menu scenes where the cursor should be visible and unlocked.
    Menu,
    // Scenes that are purely for initialization (like Bootstrap).
    System
}

[CreateAssetMenu(fileName = "SceneData_New", menuName = "Only The Shadows Know/Scene Data")]
public class SceneDataSO : ScriptableObject
{
    [Header("Scene Information")]
    public string sceneName; // The actual name of the scene file.
    public SceneType sceneType; // The type of scene this is.

    [Header("Scene Display")]
    [Tooltip("Optional: Name displayed in UI or loading screens.")]
    public string displayName;
    [TextArea]
    [Tooltip("Optional: Lore or tip text for loading screens.")]
    public string description;
}