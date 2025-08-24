using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentlyLoadedScene => _currentlyLoadedScene;
    public event Action<SceneDataSO> OnSceneLoaded;
    public event Action<SceneDataSO> OnSceneLoadCompleted;
    public event Action OnNewSceneReady;
    public event Action OnSceneReadyAndFadedIn;

    [Header("Loading Settings")]
    [SerializeField] private float minLoadTime = 2.0f;
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("How long the screen stays black AFTER the new scene is loaded but BEFORE fading in.")]
    [SerializeField] private float postLoadBlackScreenDuration = 0.5f;

    //public event Action OnSceneLoadStart;
    //public event Action OnSceneLoadEnd;
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
        if (TimeManager.Instance != null) TimeManager.Instance.ResetTimeScale();
        GameManager.Instance.UpdateGameState(GameState.Loading);

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

        /// ---MUTE SOUNDS AT THE START ---
        if (SoundEffectManager.Instance != null) SoundEffectManager.Instance.Muted = true;
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.Muted = true;

        // --- PART 1: EXIT THE CURRENT SCENE ---
        // Fade the screen (and the music) to black.
        FadeCanvas.Instance.FadeIn(fadeDuration);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // --- PART 2: THE LOADING SCENE ---
        // Load the loading scene by itself. This automatically unloads the old scene.
        Debug.Log("SCENELOADER: Unloading current scene by loading the LoadingScene...");
        yield return SceneManager.LoadSceneAsync(loadingSceneName);
        yield return null; // Wait one frame for the scene to fully initialize.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrepareForNewScene();
        }
        Debug.Log("SCENELOADER: LoadingScene is now active.");
        // Reset the audio manager's state to prevent music leaks.
        if (AudioManager.Instance != null) AudioManager.Instance.ResetAudioState();

        // Now, fade IN to the loading screen so the player can see it.
        FadeCanvas.Instance.FadeOut(fadeDuration);
        //yield return new WaitForSeconds(fadeDuration);

        // --- PART 3: LOAD THE NEW SCENE IN THE BACKGROUND ---
        // Start loading the target scene additively while the loading screen is visible.
        float startTime = Time.time;

        //AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.SceneName, LoadSceneMode.Additive);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.SceneName);
        operation.allowSceneActivation = false;

        // Update the progress bar while loading
        while (operation.progress < 0.9f)
        {
            if (LoadingScreenController.Instance != null)
                LoadingScreenController.Instance.UpdateProgress(operation.progress / 0.9f);
            yield return null;
        }
        if (LoadingScreenController.Instance != null)
            LoadingScreenController.Instance.UpdateProgress(1f);

        // Enforce the minimum load time.
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - elapsedTime);
        }

        // --- PART 4: TRANSITION TO THE NEW SCENE ---
        // Fade back to black to hide the switch.
        FadeCanvas.Instance.FadeIn(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // Activate the new scene. It's now loaded and ready.
        Debug.Log("SCENELOADER: Activating new scene...");
        operation.allowSceneActivation = true;
        Debug.Log($"SCENELOADER: New scene '{sceneToLoad.SceneName}' is now active.");
        //yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad.SceneName).isLoaded);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneToLoad.SceneName);

        //// Unload the loading scene.
        //yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        //// Officially set the new scene as active.
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad.SceneName));

        // --- PART 5: FINALIZATION ---
        _currentlyLoadedScene = sceneToLoad;

        // Set the default music for the new scene (it will be silent until we fade in).
        if (AudioManager.Instance != null && _currentlyLoadedScene.sceneMusic != null)
        {
            AudioManager.Instance.SetSceneMusic(_currentlyLoadedScene.sceneMusic, _currentlyLoadedScene.sceneMusicVolume);
        }

        OnSceneLoadCompleted?.Invoke(_currentlyLoadedScene);
        yield return new WaitForEndOfFrame();
        Debug.Log("<color=yellow>SCENELOADER: Firing OnNewSceneReady event.</color>");
        InitializeNewScene();
        OnNewSceneReady?.Invoke();

      

        if (postLoadBlackScreenDuration > 0)
        {
            yield return new WaitForSeconds(postLoadBlackScreenDuration);
        }

        // Finally, fade IN to the new scene.
        FadeCanvas.Instance.FadeOut(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);


        // --- INVOKE THE NEW EVENT HERE, after the fade is complete ---
        OnSceneReadyAndFadedIn?.Invoke();

        // --- UNMUTE SOUNDS AT THE VERY END ---
        if (SoundEffectManager.Instance != null) SoundEffectManager.Instance.Muted = false;
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.Muted = false;


        // Update the game state now that the scene is visible.
        switch (sceneToLoad.sceneType)
        {
            case SceneType.Menu:
                GameManager.Instance.UpdateGameState(GameState.Menu);
                break;
            case SceneType.Gameplay:
                GameManager.Instance.UpdateGameState(GameState.Gameplay);
                break;
        }
        _isLoading = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLoadType(GameLoadType.None);
        }
    }

    public void RestartCurrentScene()
    {
        if (_currentlyLoadedScene != null)
        {
            LoadScene(_currentlyLoadedScene, _currentlyLoadedScene.defaultSpawnPointTag);
        }
    }
    private void InitializeNewScene()
    {
        Debug.Log("<color=cyan>SceneLoader: Initializing new scene content...</color>");

        // Find and register all patrol routes
        var patrolRoutes = FindObjectsByType<PatrolRoute>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var route in patrolRoutes)
        {
            PatrolRouteManager.Instance.RegisterRoute(route);
        }
        PatrolRouteManager.Instance.SetIsReady(true);

        // Find and register all spawn points
        var spawnPoints = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                            .OfType<ISpawnPoint>();
        foreach (var point in spawnPoints)
        {
            GlobalSpawnRegistry.Instance.RegisterSpawnPoint(point);
        }
        GlobalSpawnRegistry.Instance.SetIsReady(true);

        Debug.Log("<color=cyan>SceneLoader: New scene content initialized.</color>");
    }
}