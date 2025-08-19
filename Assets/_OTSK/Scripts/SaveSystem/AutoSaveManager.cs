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
        // Subscribe to the event when this manager is enabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady += HandlePlayerReady;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady -= HandlePlayerReady;
        }
    }

    // This is now our event handler
    private void HandlePlayerReady()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.IsLoading)
        {
            Debug.Log("<color=yellow>Skipping autosave because a game load is in progress.</color>");
            return;
        }
        // Get the current scene data from the SceneLoader
        var sceneData = SceneLoader.Instance.CurrentlyLoadedScene;
        if (sceneData == null) return;

        // Now it's safe to perform the autosave
        if (sceneData.sceneType == SceneType.Gameplay)
        {
            if (SaveLoadManager.Instance != null)
            {
                Debug.Log($"<color=cyan>AUTOSAVING (Player Ready)...</color> Scene: {sceneData.SceneName}");
                SaveLoadManager.Instance.SaveGame("autosave");
            }
        }
    }
}