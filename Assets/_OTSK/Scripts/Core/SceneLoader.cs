using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentlyLoadedScene => _currentlyLoadedScene;
    public event Action<SceneDataSO> OnSceneLoaded;
    public event Action<SceneDataSO> OnSceneLoadCompleted;
    public event Action OnNewSceneReady;

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

        // --- PART 1: EXIT THE CURRENT SCENE ---
        // Fade the screen (and the music) to black.
        FadeCanvas.Instance.FadeIn(fadeDuration);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // --- PART 2: THE LOADING SCENE ---
        // Load the loading scene by itself. This automatically unloads the old scene.
        yield return SceneManager.LoadSceneAsync(loadingSceneName);

        // Reset the audio manager's state to prevent music leaks.
        if (AudioManager.Instance != null) AudioManager.Instance.ResetAudioState();

        // Now, fade IN to the loading screen so the player can see it.
        FadeCanvas.Instance.FadeOut(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // --- PART 3: LOAD THE NEW SCENE IN THE BACKGROUND ---
        // Start loading the target scene additively while the loading screen is visible.
        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.SceneName, LoadSceneMode.Additive);
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
        operation.allowSceneActivation = true;
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad.SceneName).isLoaded);

        // Unload the loading scene.
        yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        // Officially set the new scene as active.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad.SceneName));

        // --- PART 5: FINALIZATION ---
        _currentlyLoadedScene = sceneToLoad;

        // Set the default music for the new scene (it will be silent until we fade in).
        if (AudioManager.Instance != null && _currentlyLoadedScene.sceneMusic != null)
        {
            AudioManager.Instance.SetSceneMusic(_currentlyLoadedScene.sceneMusic, _currentlyLoadedScene.sceneMusicVolume);
        }

        OnSceneLoadCompleted?.Invoke(_currentlyLoadedScene);
        yield return new WaitForEndOfFrame();
        OnNewSceneReady?.Invoke();

        if (postLoadBlackScreenDuration > 0)
        {
            yield return new WaitForSeconds(postLoadBlackScreenDuration);
        }

        // Finally, fade IN to the new scene.
        FadeCanvas.Instance.FadeOut(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

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
    }

    public void RestartCurrentScene()
    {
        if (_currentlyLoadedScene != null)
        {
            LoadScene(_currentlyLoadedScene, _currentlyLoadedScene.defaultSpawnPointTag);
        }
    }
}