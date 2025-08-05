// GameManager.cs - UPGRADED VERSION

using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController Player { get; private set; }
     public Transform AimTarget { get; private set; }

    [Header("Game Start Settings")]
    [SerializeField] private SceneDataSO initialScene;

    // --- Game State Management ---
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;


    public event Action<PlayerController> OnPlayerRegistered;

    private CameraManager _cameraManager;

    void Awake()
    {
        // ... (Singleton and DontDestroyOnLoad logic remains the same) ...
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to the SceneLoader event
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded += OnSceneWasLoaded;
        }

        // Load the very first scene
        if (initialScene != null)
        {
            SceneLoader.Instance.LoadSceneAsync(initialScene);
        }
    }

    // This is the new central method for changing the game state.
    public void UpdateGameState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Game State changed to: {newState}");
    }

    private void OnSceneWasLoaded(SceneDataSO sceneData)
    {
        // When a new scene is loaded, update the game state accordingly.
        switch (sceneData.sceneType)
        {
            case SceneType.Menu:
                UpdateGameState(GameState.Menu);
                break;
            case SceneType.Gameplay:
                UpdateGameState(GameState.Gameplay);
                break;
                // The Loading state is handled by the SceneLoader itself.
        }
        Debug.Log($"GameManager: Scene '{sceneData.sceneName}' was loaded.");
        if (sceneData.objectiveChain != null)
        {
            ObjectiveManager.Instance.InitializeObjective(sceneData.objectiveChain);
        }
       
    }

    private void OnDestroy()
    {
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

        // Fire the event to notify all listening systems that the player is ready.
        OnPlayerRegistered?.Invoke(player);

        Debug.Log("GameManager: Player reference has been registered and announced.");

        if (_cameraManager != null)
        {
            _cameraManager.ConnectToPlayer(player);
        }
    }

    public void RegisterCameraManager(CameraManager camManager)
    {
        _cameraManager = camManager;
    }

    public void RegisterAimTarget(Transform target)
    {
        AimTarget = target;
    }
}
