// GameManager.cs - UPGRADED VERSION

using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController Player { get; private set; }

    [Header("Game Start Settings")]
    [SerializeField] private SceneDataSO initialScene;

    // --- Game State Management ---
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

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
        Player = player;
    }
}