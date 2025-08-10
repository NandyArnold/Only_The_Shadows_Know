using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // Required for SceneAsset
#endif

// Define the different types of scenes we can have.
public enum SceneType
{
    Gameplay,
    Menu,
    System // e.g., Bootstrap, Initialization
}

[CreateAssetMenu(fileName = "SceneData_New", menuName = "Only The Shadows Know/Scene Data")]
public class SceneDataSO : ScriptableObject
{
    [Header("Scene Information")]
    [Tooltip("A unique, permanent ID for this scene. Used for saving and loading.")]
    public string sceneID;

    // This is the scene file itself. It can only be used in the Editor.
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    // This is the scene name, used by the SceneManager at runtime.
    [Tooltip("The name of the scene file. This is set automatically from the Scene Asset.")]
    [SerializeField] private string sceneName;

    // Public property to safely access the scene name.
    public string SceneName => sceneName;
    
    public SceneType sceneType;

    [Header("Scene Display")]
    [Tooltip("Optional: Name displayed in UI or loading screens.")]
    public string displayName;
    [TextArea]
    [Tooltip("Optional: Lore or tip text for loading screens.")]
    public string description;

    [Header("Level Logic")]
    public LevelObjectiveChainSO objectiveChain;
    public string defaultSpawnPointTag = "InitialSpawn";

    [Header("Enemy Spawning")]
    public List<SpawnData> enemyInitialSpawns; // Renamed for clarity

    // This special Unity method runs in the Editor whenever you change a value.
    private void OnValidate()
    {
#if UNITY_EDITOR
        // It automatically updates the sceneName string when you assign the sceneAsset.
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
#endif
    }
}