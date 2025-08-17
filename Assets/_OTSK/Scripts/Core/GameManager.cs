// GameManager.cs - UPGRADED VERSION

using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController Player { get; private set; }
    public AimTargetController AimTargetController { get; private set; }
    public Transform AimTarget { get; private set; }

    [Header("Game Start Settings")]
    [SerializeField] private SceneDataSO initialScene;

    // --- Game State Management ---
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;
    public event Action OnPlayerReady;

    public event Action<PlayerController> OnPlayerRegistered;

    private CameraManager _cameraManager;
    private PlayerInputHandler _playerInputHandler;

    void Awake()
    {
        // ... (Singleton logic remains the same) ...
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    
    }
    private IEnumerator Start()
    {
        // Wait for the end of the very first frame.
        // This gives all other managers a chance to run their Awake() and Start() methods
        // and subscribe to any necessary events.
        yield return null;

        // Now it is safe to load the initial scene.
        if (initialScene != null && SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(initialScene, initialScene.defaultSpawnPointTag);
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


    private void OnDestroy()
    {
      
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnPauseInput -= HandlePauseInput;
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
        _playerInputHandler = player.GetComponent<PlayerInputHandler>();
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnPauseInput += HandlePauseInput;
        }
    }

    public void RegisterCameraManager(CameraManager camManager)
    {
        _cameraManager = camManager;
    }

    public void RegisterAimTarget(AimTargetController controller)
    {
        AimTargetController = controller;
        AimTarget = controller.transform; 
    }


    private void HandlePauseInput()
    {
        if (CurrentState == GameState.Gameplay)
        {
            UpdateGameState(GameState.Menu);
        }
        else if (CurrentState == GameState.Menu)
        {
            // Instead of calling UpdateGameState directly, we start a coroutine.
            StartCoroutine(UnpauseRoutine());
        }
    }
    private IEnumerator UnpauseRoutine()
    {
        // 1. Tell the system to go back to Gameplay. This will hide the menu UI.
        UpdateGameState(GameState.Gameplay);

        // 2. Wait for the end of the frame. This gives the UI EventSystem time to release focus.
        yield return new WaitForEndOfFrame();

        // 3. Just to be safe, we re-apply the Gameplay cursor state one more time.
        //    This "double tap" ensures the cursor locks and hides correctly.
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetState(CursorState.Gameplay);
        }
    }

    public void RespawnPlayer()
    {
        if (Player == null) return;

        // 1. Find the last checkpoint from the manager
        var checkpointManager = CheckpointManager.Instance;
        if (checkpointManager.LastCheckpointPosition == Vector3.zero)
        {
            Debug.Log("No checkpoint found. Loading last autosave to restart level.");
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.LoadGame("autosave");
            }
            return;
        }

        // 2. Tell the PlayerController to move to the checkpoint
        Player.RespawnAt(checkpointManager.LastCheckpointPosition, checkpointManager.LastCheckpointRotation);

        // 3. Tell the PlayerStats to revive (reset health and animation)
        Player.GetComponent<PlayerStats>().Revive();

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ResetAllEnemies();
        }

        // 4. Tell the EnemyManager to wake up all enemies.
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnPlayerRespawn();
        }
        // 5. Set the game state back to Gameplay
        UpdateGameState(GameState.Gameplay);
    }
    public void AnnouncePlayerReady()
    {
        OnPlayerReady?.Invoke();
    }

    public void InstakillPlayer()
    {
        if (Player != null && Player.GetComponent<PlayerStats>() != null)
        {
            // Deal a huge amount of damage that cannot be survived.
            Player.GetComponent<PlayerStats>().TakeDamage(float.MaxValue);
        }
    }
}
