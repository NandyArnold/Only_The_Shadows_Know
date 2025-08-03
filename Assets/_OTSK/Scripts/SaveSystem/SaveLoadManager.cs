// SaveLoadManager.cs - ESave Powered Version
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Esper.ESave; // Add the ESave namespace

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("ESave Configuration")]
    [SerializeField] private SaveFileSetup saveFileSetup; // Assign this in the Inspector

    private SaveFile _saveFile;
    private GameStateData _currentGameState;
    private const string GameStateKey = "GameState"; // The ID for our main data object

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (saveFileSetup != null)
        {
            _saveFile = saveFileSetup.GetSaveFile();
        }
        else
        {
            Debug.LogError("SaveFileSetup is not assigned on the SaveLoadManager!", this);
        }
    }

    public void SaveGame(string saveFileName = "save_slot_1")
    {
        if (_saveFile == null)
        {
            Debug.LogError("Save file is not initialized!");
            return;
        }

        _currentGameState = new GameStateData();

        GatherPlayerData();
        GatherObjectiveData();
        GatherWorldData();

        _saveFile.AddOrUpdateData(GameStateKey, _currentGameState);
        _saveFile.Save();

        Debug.Log($"<color=green>Game Saved via ESave:</color> {saveFileName}");
    }

    public void LoadGame(string saveFileName = "save_slot_1")
    {
        if (_saveFile == null)
        {
            Debug.LogError("Save file is not initialized!");
            return;
        }

        if (_saveFile.HasData(GameStateKey))
        {
            _currentGameState = _saveFile.GetData<GameStateData>(GameStateKey);

            RestorePlayerData();
            RestoreObjectiveData();
            RestoreWorldData();

            Debug.Log($"<color=cyan>Game Loaded via ESave:</color> {saveFileName}");
        }
        else
        {
            Debug.LogWarning($"Save file found, but no GameState data was present.");
        }
    }

    private void GatherObjectiveData()
    {
        if (ObjectiveManager.Instance != null)
        {
            _currentGameState.objectiveData = ObjectiveManager.Instance.CaptureState();
        }
    }

    private void GatherPlayerData()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var stats = player.GetComponent<PlayerHealthManaNoise>();

        // Convert Unity types to ESave's savable types
        _currentGameState.playerData.position = player.transform.position.ToSavable();
        _currentGameState.playerData.rotation = player.transform.rotation.ToSavable();
        _currentGameState.playerData.currentHealth = stats.CurrentHealth;
        _currentGameState.playerData.currentMana = stats.CurrentMana;
    }

    private void RestorePlayerData()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;

        // Convert ESave's types back to Unity types
        player.transform.position = _currentGameState.playerData.position.vector3Value;
        player.transform.rotation = _currentGameState.playerData.rotation.quaternionValue;

        controller.enabled = true;
        var stats = player.GetComponent<PlayerHealthManaNoise>();
        stats.RestoreStats(_currentGameState.playerData.currentHealth, _currentGameState.playerData.currentMana);
    }

    private void RestoreObjectiveData()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RestoreState(_currentGameState.objectiveData);
        }
    }
    private void GatherWorldData()
    {
        // Find all objects in the scene that implement the ISaveable interface.
        var saveableEntities = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ISaveable>();

        var worldState = new Dictionary<string, object>();
        foreach (var entity in saveableEntities)
        {
            // For each object, capture its state and store it in a dictionary with its unique ID.
            worldState[entity.UniqueID] = entity.CaptureState();
        }

        _currentGameState.worldData = new WorldStateData(worldState);
    }

    private void RestoreWorldData()
    {
        var saveableEntities = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ISaveable>();
        var worldState = _currentGameState.worldData.ToDictionary();

        foreach (var entity in saveableEntities)
        {
            // For each saveable object in the scene, find its saved data using its unique ID.
            if (worldState.TryGetValue(entity.UniqueID, out object state))
            {
                // If data is found, tell the object to restore its state.
                entity.RestoreState(state);
            }
        }
    }
}