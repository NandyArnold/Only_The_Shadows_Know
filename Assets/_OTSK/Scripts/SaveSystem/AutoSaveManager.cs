// In AutosaveManager.cs
using UnityEngine;

public class AutosaveManager : MonoBehaviour
{
    public static AutosaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneReadyAndFadedIn += HandleSceneReadyForAutosave;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneReadyAndFadedIn -= HandleSceneReadyForAutosave;
        }
    }

    // This is now our event handler
    private void HandleSceneReadyForAutosave()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.IsLoading)
        {
            Debug.Log("<color=yellow>Skipping autosave because a game load is in progress.</color>");
            return;
        }

        var sceneData = SceneLoader.Instance.CurrentlyLoadedScene;
        if (sceneData == null || sceneData.sceneType != SceneType.Gameplay) return;

        if (SaveLoadManager.Instance != null)
        {
            Debug.Log($"<color=cyan>AUTOSAVING (Scene Faded In)...</color> Scene: {sceneData.SceneName}");
            SaveLoadManager.Instance.SaveGame("autosave");
        }
    }
}