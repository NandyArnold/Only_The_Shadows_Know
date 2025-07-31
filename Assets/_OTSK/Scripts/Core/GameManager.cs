// GameManager.cs - CORRECTED VERSION

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController Player { get; private set; }

    [Header("Game Start Settings")]
    [Tooltip("The SceneData for the initial scene to load after Bootstrap.")]
    [SerializeField] private SceneDataSO initialScene; // Use this instead of a string

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("GameManager Initialized.");

        // Subscribe to the SceneLoader event using the correct signature.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded += OnSceneWasLoaded;

            // Call the correct LoadSceneAsync method with the SceneDataSO.
            if (initialScene != null)
            {
                SceneLoader.Instance.LoadSceneAsync(initialScene);
            }
            else
            {
                Debug.LogError("GameManager: Initial Scene Data is not set in the Inspector!", this);
            }
        }
        else
        {
            Debug.LogError("GameManager: SceneLoader.Instance is null.", this);
        }
    }

    // This method now correctly accepts a SceneDataSO parameter.
    private void OnSceneWasLoaded(SceneDataSO sceneData)
    {
        Debug.Log($"GameManager: Scene '{sceneData.sceneName}' of type '{sceneData.sceneType}' was loaded.");

        // Here you can add logic based on scene type or name.
        // For example, find and initialize the PlayerSpawner only in Gameplay scenes.
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded -= OnSceneWasLoaded;
        }
    }

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