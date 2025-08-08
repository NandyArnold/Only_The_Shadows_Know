using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentSceneData { get; private set; }
    public event Action<SceneDataSO> OnSceneLoaded;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadTime = 2.0f;


    public event Action OnSceneLoadStart;
    public event Action OnSceneLoadEnd;
    //public event Action<string> OnSceneUnloaded;

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
            LoadScene(CurrentSceneData, CurrentSceneData.defaultSpawnPointTag, true);
        }
    }

    public void LoadScene(SceneDataSO sceneData, string spawnPointTag, bool showLoadingScreen = true)
    {
        if (_isLoading) return;
        _isLoading = true;

        // Tell the PlayerSpawner where to spawn in the next scene
        if (PlayerSpawner.Instance != null)
        {
            PlayerSpawner.Instance.SetNextSpawnPoint(spawnPointTag);
        }

        _sceneDataToLoad = sceneData;
        StartCoroutine(LoadSceneCoroutine(sceneData.sceneName, spawnPointTag, showLoadingScreen));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, string spawnPointTag, bool showLoadingScreen)
    {
        try
        {
            // Tell the PlayerSpawner where to spawn in the next scene.
            if (PlayerSpawner.Instance != null)
            {
                PlayerSpawner.Instance.SetNextSpawnPoint(spawnPointTag);
            }

            if (showLoadingScreen)
            {
                OnSceneLoadStart?.Invoke();
                yield return new WaitForSecondsRealtime(0.1f);
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }

            OnSceneLoaded?.Invoke(_sceneDataToLoad);
        }
        finally
        {
            _isLoading = false;
            if (showLoadingScreen)
            {
                OnSceneLoadEnd?.Invoke();
            }
        }
    }
}