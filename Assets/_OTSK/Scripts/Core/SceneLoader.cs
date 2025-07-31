using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections; // Required for IEnumerator

public class SceneLoader : MonoBehaviour
{
    // Public static property to access the single instance of SceneLoader
    public static SceneLoader Instance { get; private set; }

    // Events that other scripts can subscribe to
    public event Action<SceneDataSO> OnSceneLoaded;
    public event Action<string> OnSceneUnloaded;

    private SceneDataSO _currentSceneData; // To hold the data of the scene being loaded.

    // --- Singleton Initialization ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // If another instance already exists, destroy this one.
            // This ensures only one SceneLoader persists.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Make this GameObject persist across scene loads
        DontDestroyOnLoad(gameObject);

        //Debug.Log("SceneLoader Initialized.");
    }

    // --- Scene Loading Method ---
    public void LoadSceneAsync(SceneDataSO sceneData, string spawnPointTag = "")
    {
        if (sceneData == null)
        {
            Debug.LogError("SceneLoader: Cannot load a null SceneDataSO!");
            return;
        }
        _currentSceneData = sceneData; // Store the data
        Debug.Log($"SceneLoader: Attempting to load scene: {sceneData} with spawn tag: {spawnPointTag}");
        StartCoroutine(LoadSceneCoroutine(sceneData.sceneName, spawnPointTag));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, string spawnPointTag)
    {
        // 1. Unload current scene (if not Bootstrap or LoadingScreen)
        //    For simplicity in this initial step, we won't explicitly unload
        //    the current scene first if we're going straight from one game scene to another.
        //    Unity will handle unloading the previous scene when a new one loads additively or singularly.

        // 2. (Optional but recommended): Load LoadingScreen scene first for smooth transition
        //    This is crucial for larger games. For now, we'll keep it simple and just load the target scene.
        //    Later, you can add logic here to load your specific "LoadingScreen" scene.
        //    Example: if (SceneManager.GetActiveScene().name != "LoadingScreen")
        //                 yield return SceneManager.LoadSceneAsync("LoadingScreen");

        // 3. Start loading the target scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent auto-activation until we're ready

        // While the scene is loading but not yet ready to activate
        while (!operation.isDone)
        {
            // Update loading progress (for a loading bar UI later)
            // You can access operation.progress (0.0 to 0.9)
            // Debug.Log($"Loading progress: {operation.progress * 100}%");

            if (operation.progress >= 0.9f)
            {
                // Scene is loaded, but not activated yet.
                // We could wait for user input or other conditions here.
                // For now, activate immediately.
                operation.allowSceneActivation = true;
            }
            yield return null; // Wait for the next frame
        }

        // Scene is now active
        Debug.Log($"SceneLoader: Scene '{sceneName}' loaded and activated.");

        // Invoke events AFTER scene is fully loaded and activated
        OnSceneLoaded?.Invoke(_currentSceneData);

        // Store spawnPointTag temporarily if needed, e.g., in GameManager, or pass directly
        // For now, it's passed but not used by SceneLoader itself.
        // The GameManager/PlayerSpawner will pick this up.
    }

    // --- Scene Unloading Method (Optional, useful for additive loading) ---
    // You might not need this explicitly for single-scene transitions,
    // as Unity unloads the previous scene automatically.
    // However, it's good practice for when you additively load scenes later.
    public void UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            StartCoroutine(UnloadSceneCoroutine(sceneName));
        }
    }

    private IEnumerator UnloadSceneCoroutine(string sceneName)
    {
        Debug.Log($"SceneLoader: Attempting to unload scene: {sceneName}");
        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            yield return null; // Wait for the next frame
        }

        Debug.Log($"SceneLoader: Scene '{sceneName}' unloaded.");
        OnSceneUnloaded?.Invoke(sceneName);
    }
}
