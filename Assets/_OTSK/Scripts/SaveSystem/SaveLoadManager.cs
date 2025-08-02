using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private GameStateData _currentGameState;
    private string _savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Use the fully qualified name to avoid conflicts
        _savePath = System.IO.Path.Combine(Application.persistentDataPath, "saves");
        if (!System.IO.Directory.Exists(_savePath))
        {
            System.IO.Directory.CreateDirectory(_savePath);
        }
    }

    public void SaveGame(string saveFileName)
    {
        _currentGameState = new GameStateData();

        GatherPlayerData();
        GatherObjectiveData();
        GatherWorldData();

        string json = JsonUtility.ToJson(_currentGameState, true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(_savePath, saveFileName + ".json"), json);

        Debug.Log($"<color=green>Game Saved:</color> {saveFileName}.json");
    }

    public void LoadGame(string saveFileName)
    {
        string filePath = System.IO.Path.Combine(_savePath, saveFileName + ".json");
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            _currentGameState = JsonUtility.FromJson<GameStateData>(json);

            RestorePlayerData();
            RestoreObjectiveData();
            RestoreWorldData();

            Debug.Log($"<color=cyan>Game Loaded:</color> {saveFileName}.json");
        }
        else
        {
            Debug.LogWarning($"Save file not found: {filePath}");
        }
    }

    // --- (The rest of the script is unchanged) ---
    #region Data Gathering and Restoring
    private void GatherPlayerData()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        _currentGameState.playerData.position = player.transform.position;
        _currentGameState.playerData.rotation = player.transform.rotation;

        var stats = player.GetComponent<PlayerHealthManaNoise>();
        _currentGameState.playerData.currentHealth = stats.CurrentHealth;
        _currentGameState.playerData.currentMana = stats.CurrentMana;
    }

    private void GatherObjectiveData() { /* TODO */ }

    private void GatherWorldData()
    {
        var saveableEntities = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ISaveable>();
        var worldState = new Dictionary<string, object>();
        foreach (var entity in saveableEntities)
        {
            worldState[entity.UniqueID] = entity.CaptureState();
        }
        _currentGameState.worldData = new WorldStateData(worldState);
    }

    private void RestorePlayerData()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = _currentGameState.playerData.position;
        player.transform.rotation = _currentGameState.playerData.rotation;
        controller.enabled = true;
    }

    private void RestoreObjectiveData() { /* TODO */ }

    private void RestoreWorldData()
    {
        var saveableEntities = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ISaveable>();
        var worldState = _currentGameState.worldData.ToDictionary();

        foreach (var entity in saveableEntities)
        {
            if (worldState.TryGetValue(entity.UniqueID, out object state))
            {
                entity.RestoreState(state);
            }
        }
    }
    #endregion
}