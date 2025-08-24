// In AutosaveManager.cs
using UnityEngine;

public class AutosaveManager : MonoBehaviour
{
    public static AutosaveManager Instance { get; private set; }
    private SceneDataSO _currentSceneData;

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
            SceneLoader.Instance.OnSceneLoadCompleted += OnSceneCompleted;
            SceneLoader.Instance.OnSceneReadyAndFadedIn += HandleSceneReadyForAutosave;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted -= OnSceneCompleted;
            SceneLoader.Instance.OnSceneReadyAndFadedIn -= HandleSceneReadyForAutosave;
        }
    }

    // This is now our event handler
    private void HandleSceneReadyForAutosave()
    {
        if (_currentSceneData != null && _currentSceneData.sceneType != SceneType.Gameplay)
        {
            Debug.Log($"AutosaveManager: Suppressing autosave because current scene '{_currentSceneData.SceneName}' is not a Gameplay scene.");
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.CurrentLoadType == GameLoadType.LoadFromSave)
        {
            Debug.Log("<color=orange>AutosaveManager: Suppressing post-load autosave.</color>");
            return;
        }
        if (GameManager.Instance != null && GameManager.Instance.CurrentLoadType == GameLoadType.NewGame)
        {
            Debug.Log("AutosaveManager: Suppressing initial autosave on New Game start. EnemySpawner will handle it.");
            return;
        }

        // This will now only run on subsequent scene loads or after a LoadFromSave is complete.
        Debug.Log($"<color=cyan>AUTOSAVING (Scene Faded In)...</color>");
        StartCoroutine(SaveLoadManager.Instance.SaveGame("autosave"));
    }
    private void OnSceneCompleted(SceneDataSO sceneData)
    {
        _currentSceneData = sceneData;
    }
}