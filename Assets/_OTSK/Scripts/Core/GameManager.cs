using UnityEngine;
using UnityEngine.SceneManagement; // For SceneManager.GetActiveScene().name
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController Player { get; private set; }

    // References to other managers (will link in inspector later)
    // public UIManager UIManager; // Will create soon
    // public TeleportManager TeleportManager; // Will create soon
    // public SkillManager SkillManager; // Will create soon
    // public ObjectiveManager ObjectiveManager; // Will create soon

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Make sure GameManager also persists

        Debug.Log("GameManager Initialized.");

        // --- Initial Scene Load ---
        // If we're not already in the MainMenu (e.g., first start from Bootstrap)
        // Or if we need to enforce starting at MainMenu after a build.
        if (SceneManager.GetActiveScene().name == "BootstrapScene") // Or your initial scene name
        {
            // This will trigger SceneLoader.OnSceneLoaded which GameManager can subscribe to
            // to do scene-specific setup.
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneAsync("MainMenu");
                SceneLoader.Instance.OnSceneLoaded += OnSceneWasLoaded; // Subscribe here
            }
            else
            {
                Debug.LogError("GameManager: SceneLoader.Instance is null. Ensure SceneLoader starts before GameManager.");
            }
        }
    }

    private void OnSceneWasLoaded(string sceneName)
    {
        Debug.Log($"GameManager: Scene '{sceneName}' was loaded.");
        // Here you can add logic that needs to happen when a scene loads
        // e.g., PlayerSpawner.Instance.SpawnPlayerAtTag(currentSpawnPointTag);
        // UIManager.InitSceneUI(sceneName);
        // ObjectiveManager.InitLevelObjectives(sceneName);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks if this object is ever destroyed
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded -= OnSceneWasLoaded;
        }
    }

    // Add a public method for the player to register itself.
    public void RegisterPlayer(PlayerController player)
    {
        if (Player != null)
        {
            Debug.LogWarning("GameManager: A player is already registered. Overwriting reference.");
        }
        Player = player;
        Debug.Log("GameManager: Player reference has been registered.");
    }
}
