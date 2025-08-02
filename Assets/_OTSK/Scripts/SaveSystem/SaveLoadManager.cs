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

    public void SaveGame()
    {
        if (_saveFile == null) return;

        _currentGameState = new GameStateData();

        GatherPlayerData();
        // ... GatherObjectiveData() and GatherWorldData() will be called here ...

        _saveFile.AddOrUpdateData(GameStateKey, _currentGameState);
        _saveFile.Save();

        //Debug.Log($"<color=green>Game Saved via ESave:</color> {saveFileSetup.fileName}");
    }

    public void LoadGame()
    {
        if (_saveFile == null) return;

        if (_saveFile.HasData(GameStateKey))
        {
            _currentGameState = _saveFile.GetData<GameStateData>(GameStateKey);

            RestorePlayerData();
            // ... RestoreObjectiveData() and RestoreWorldData() will be called here ...

            //Debug.Log($"<color=cyan>Game Loaded via ESave:</color> {saveFileSetup.fileName}");
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

        // TODO: Restore Health/Mana by adding a "SetStats" method to PlayerHealthManaNoise
    }

    // --- (Gather/Restore for Objectives and World Data remain the same) ---
}