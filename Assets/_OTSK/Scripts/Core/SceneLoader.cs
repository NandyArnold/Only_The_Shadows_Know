using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public SceneDataSO CurrentlyLoadedScene => _currentlyLoadedScene;
    public event Action<SceneDataSO> OnSceneLoaded;

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
        // 1. Fade TO black
        FadeCanvas.Instance.FadeIn(fadeDuration);
        GameManager.Instance.UpdateGameState(GameState.Loading);
        yield return new WaitForSeconds(fadeDuration);

        // 2. Load the Loading Scene
        yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

        // 3. Unload the OLD scene
        if (_currentlyLoadedScene != null)
        {
            yield return SceneManager.UnloadSceneAsync(_currentlyLoadedScene.SceneName);
        }

        // 4. Load the NEW scene in the background
        float startTime = Time.realtimeSinceStartup;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.SceneName, LoadSceneMode.Additive);
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
        yield return new WaitUntil(() => SceneManager.GetSceneByName(sceneToLoad.SceneName).isLoaded);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad.SceneName));
        yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        // --- FINALIZATION ---
        // The player will be spawned here, but the screen is still black.
        _currentlyLoadedScene = sceneToLoad;
        OnSceneLoaded?.Invoke(_currentlyLoadedScene);// This will spawn the player

        yield return null; // Wait for the player to be spawned

        //  NOW it is safe to perform the autosave.
        if (AutosaveManager.Instance != null)
        {
            AutosaveManager.Instance.PerformAutosave(sceneToLoad);
        }

        // 7. Hold the black screen for the specified duration.
        if (postLoadBlackScreenDuration > 0)
        {
            yield return new WaitForSeconds(postLoadBlackScreenDuration);
        }

        // 8. Fade FROM black.
        FadeCanvas.Instance.FadeOut(fadeDuration);
        yield return new WaitForSecondsRealtime(fadeDuration);

        // 9. NOW, with the scene visible, update the game state to Gameplay.
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