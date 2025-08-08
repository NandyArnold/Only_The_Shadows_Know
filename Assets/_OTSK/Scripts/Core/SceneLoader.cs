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
    [SerializeField] private string loadingSceneName = "LoadingScene";

    public event Action OnSceneLoadStart;
    public event Action OnSceneLoadEnd;
    //public event Action<string> OnSceneUnloaded;

    private SceneDataSO _currentlyLoadedScene;
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
        if (_currentlyLoadedScene != null)
        {
            LoadScene(_currentlyLoadedScene, _currentlyLoadedScene.defaultSpawnPointTag);
        }
    }

    public void LoadScene(SceneDataSO sceneData, string spawnPointTag = null)
    {
        if (_isLoading) return;
        
        

        // Tell the PlayerSpawner where to spawn in the next scene
        if (PlayerSpawner.Instance != null)
        {
            PlayerSpawner.Instance.SetNextSpawnPoint(spawnPointTag);
        }

        StartCoroutine(LoadSceneRoutine(sceneData));
    }

    private IEnumerator LoadSceneRoutine(SceneDataSO sceneToLoad)
    {
        _isLoading = true;
        float startTime = Time.realtimeSinceStartup;

        // --- PREPARE ---
        // 1. Load the Loading Scene additively.
        yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

        // --- UNLOAD OLD SCENE ---
        // 2. If there was a previous scene, start unloading it.
        AsyncOperation unloadOperation = null;
        // Only try to unload a scene if there IS a previously loaded scene.
        if (_currentlyLoadedScene != null)
        {
            unloadOperation = SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneName);
        }


        // --- LOAD NEW SCENE ---
        // 3. Start loading the actual target scene in the background.
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad.sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        // 4. Wait for unloading and loading to complete.
        while (unloadOperation != null && !unloadOperation.isDone) { yield return null; }
        while (loadOperation.progress < 0.9f)
        {
            if (LoadingScreenController.Instance != null)
                LoadingScreenController.Instance.UpdateProgress(loadOperation.progress / 0.9f);
            yield return null;
        }

        if (LoadingScreenController.Instance != null)
            LoadingScreenController.Instance.UpdateProgress(1f);

        // 5. Enforce the minimum load time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minLoadTime)
        {
            yield return new WaitForSecondsRealtime(minLoadTime - elapsedTime);
        }

        // 6. Activate the new scene.
        loadOperation.allowSceneActivation = true;
        // Wait for the new scene to be fully active
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad.sceneName).isLoaded);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad.sceneName));

        // 7. Unload the loading scene.
        yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        // --- COMPLETE ---
        _currentlyLoadedScene = sceneToLoad;
        OnSceneLoaded?.Invoke(_currentlyLoadedScene);
        _isLoading = false;
    }


}