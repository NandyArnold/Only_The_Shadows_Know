using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using static UnityEngine.Rendering.HDROutputUtils;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentSceneData { get; private set; }
    public event Action<SceneDataSO> OnSceneLoaded;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadTime = 2.0f;
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("How long the screen stays black AFTER the new scene is loaded but BEFORE fading in.")]
    [SerializeField] private float postLoadBlackScreenDuration = 0.5f;

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
        // 1. Fade TO black
        FadeCanvas.Instance.FadeIn(fadeDuration);
        if (GameManager.Instance.Player != null)
            GameManager.Instance.Player.GetComponent<PlayerInputHandler>().SwitchActionMap("Disabled");
        yield return new WaitForSeconds(fadeDuration);

        // 2. Load the Loading Scene
        yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

        // 3. Unload the OLD scene
        if (_currentlyLoadedScene != null)
        {
            yield return SceneManager.UnloadSceneAsync(_currentlyLoadedScene.sceneName);
        }

        // 4. Load the NEW scene in the background
        float startTime = Time.realtimeSinceStartup;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            if (LoadingScreenController.Instance != null)
                LoadingScreenController.Instance.UpdateProgress(operation.progress / 0.9f);
            yield return null;
        }

        if (LoadingScreenController.Instance != null)
            LoadingScreenController.Instance.UpdateProgress(1f);

        // 5. Enforce minimum load time
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (elapsedTime < minLoadTime)
        {
            yield return new WaitForSecondsRealtime(minLoadTime - elapsedTime);
        }

        // 6. Activate the new scene and unload the loading scene
        operation.allowSceneActivation = true;
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad.sceneName).isLoaded);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad.sceneName));
        yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        // --- FINALIZATION ---
        _currentlyLoadedScene = sceneToLoad;
        OnSceneLoaded?.Invoke(_currentlyLoadedScene); // This will spawn the player
        yield return null; // Wait one frame to ensure the player has been spawned and initialized

        // 7. THIS IS THE NEW DELAY: Hold the black screen for the specified duration.
        if (postLoadBlackScreenDuration > 0)
        {
            yield return new WaitForSeconds(postLoadBlackScreenDuration);
        }

        // 8. Fade FROM black and re-enable input
        FadeCanvas.Instance.FadeOut(fadeDuration);
        if (GameManager.Instance.Player != null)
            GameManager.Instance.Player.GetComponent<PlayerInputHandler>().SwitchActionMap("Player");

        _isLoading = false;
    }

    public void RestartCurrentScene()
    {
        if (_currentlyLoadedScene != null)
        {
            LoadScene(_currentlyLoadedScene, _currentlyLoadedScene.defaultSpawnPointTag);
        }
    }
}