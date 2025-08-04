using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentSceneData { get; private set; }
    public event Action<SceneDataSO> OnSceneLoaded;
    public event Action<string> OnSceneUnloaded;

    private SceneDataSO _sceneDataToLoad;
    private bool _isLoading = false; // The new flag to prevent duplicate loads

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RestartCurrentScene()
    {
        if (CurrentSceneData != null)
        {
            LoadSceneAsync(CurrentSceneData);
        }
    }

    public void LoadSceneAsync(SceneDataSO sceneData, string spawnPointTag = "")
    {
        // Guard Clause: If we are already loading a scene, ignore this new request.
        if (_isLoading)
        {
            Debug.LogWarning($"SceneLoader is already loading a scene. Ignoring request to load {sceneData.sceneName}.");
            return;
        }

        _isLoading = true;
        GameManager.Instance.UpdateGameState(GameState.Loading);
        _sceneDataToLoad = sceneData;
        StartCoroutine(LoadSceneCoroutine(sceneData.sceneName, spawnPointTag));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, string spawnPointTag)
    {
        try
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }
                yield return null;
            }

            CurrentSceneData = _sceneDataToLoad;
            OnSceneLoaded?.Invoke(CurrentSceneData);
        }
        finally
        {
            // This 'finally' block ensures that _isLoading is always reset to false,
            // even if an error occurs during loading.
            _isLoading = false;
        }
    }
}