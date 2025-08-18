// SaveLoadManager.cs - ESave Powered Version
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Esper.ESave; // Add the ESave namespace
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using Esper.ESave.SavableObjects; // Add the ESave Data namespace 
using static Esper.ESave.SaveFileSetupData;



public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Data Registries")] 
    [SerializeField] private SceneRegistrySO sceneRegistry;

    [Header("ESave Configuration")]
    [SerializeField] private SaveFileSetup saveFileSetup; // Assign this in the Inspector

    private SaveFile _metaFile;
    private const string LastSaveKey = "LastSaveName";

    private SaveFile _saveFile;
    private GameStateData _currentGameState;
    private const string GameStateKey = "GameState"; // The ID for our main data object

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        var metaFileSetupData = new SaveFileSetupData()
        {
            fileName = "meta", // ESave will add the extension
            saveLocation = SaveLocation.PersistentDataPath,
            // We set this to false so this internal file doesn't show up in a save game list.
            addToStorage = false
        };

        // Now we can create the file correctly using the setup data.
        _metaFile = new SaveFile(metaFileSetupData);
    }

    private void Start()
    {
        if (saveFileSetup != null)
        {
            _saveFile = saveFileSetup.GetSaveFile();
            // Get a new SaveFile instance just for our metadata
            
        }
        else
        {
            Debug.LogError("SaveFileSetup is not assigned on the SaveLoadManager!", this);
        }
       
    }

    public void SaveGame(string saveFileName)
    {
        // Get a SaveFile instance for the specific file name we are saving to.
        var saveFile = new SaveFile(new SaveFileSetupData() { fileName = saveFileName });

        var currentGameState = new GameStateData();
       
        // Initialize the GameStateData object with default values.
        currentGameState.playerData = new PlayerStateData();
        if (SceneLoader.Instance.CurrentlyLoadedScene != null)
        {
            currentGameState.sceneID = SceneLoader.Instance.CurrentlyLoadedScene.sceneID;
        }

        // Pass the GameStateData object to the gather methods to be filled.
        GatherPlayerData(currentGameState);
        GatherObjectiveData(currentGameState);
        GatherWorldData(currentGameState);

        saveFile.AddOrUpdateData(GameStateKey, currentGameState);
        saveFile.Save();

        // After successfully saving, also save the name to our metadata file.
        _metaFile.AddOrUpdateData(LastSaveKey, saveFileName);
        _metaFile.Save();

        Debug.Log($"<color=green>Game Saved via ESave:</color> {saveFileName}");
    }

    public void LoadGame(string saveFileName = "save_slot_1")
    {
        if (_saveFile == null) return;
        StartCoroutine(LoadGameRoutine(saveFileName));
        
    }

    private IEnumerator LoadGameRoutine(string saveFileName)
    {
        var saveFile = new SaveFile(new SaveFileSetupData() { fileName = saveFileName });

        if (!saveFile.HasData(GameStateKey))
        {
            Debug.LogWarning($"Save file '{saveFileName}' has no GameState data.");
            yield break;
        }

        var currentGameState = saveFile.GetData<GameStateData>(GameStateKey);

        // 2. Find the correct SceneDataSO using the saved ID from the registry.
        var sceneDataToLoad = sceneRegistry.GetSceneData(_currentGameState.sceneID);
        if (sceneDataToLoad == null)
        {
            Debug.LogError($"Could not find scene with ID '{_currentGameState.sceneID}' in the SceneRegistry!");
            yield break;
        }

        // 3. Tell the SceneLoader to start loading the scene.
        //    We don't pass a spawn point tag because the save data will override the position.
        SceneLoader.Instance.LoadScene(sceneDataToLoad);

        // 4. Wait until the SceneLoader has finished its ENTIRE routine and a new player exists.
        yield return new WaitUntil(() => GameManager.Instance.Player != null);

        // 5. NOW that the new player exists in the new scene, it's safe to restore all data.
        RestorePlayerData(currentGameState);
        RestoreObjectiveData(currentGameState);
        RestoreWorldData(currentGameState);

        // 6. Connect the camera to the new player.
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ConnectToPlayer(GameManager.Instance.Player);
        }

        // 7. Revive the player to reset their animation state.
        GameManager.Instance.Player.GetComponent<PlayerStats>().Revive();

        Debug.Log($"<color=cyan>Game Loaded:</color> {saveFileName}");
    }

    private void GatherObjectiveData(GameStateData gameState)
    {
        if (ObjectiveManager.Instance != null)
        {
            gameState.objectiveData = ObjectiveManager.Instance.CaptureState();
        }
        else
        {
            Debug.LogWarning("GatherObjectiveData was called, but ObjectiveManager.Instance was null. Aborting objective save.");
        }
    }

    private void GatherPlayerData(GameStateData gameState)
    {
        // --- THIS IS THE FIX ---
        // First, get the player reference and add a safety check.
        var player = GameManager.Instance.Player;
        if (player == null)
        {
            Debug.LogWarning("GatherPlayerData was called, but GameManager.Instance.Player was null. Aborting player save.");
            return;
        }
        // -----------------------

        var stats = player.GetComponent<PlayerStats>();

        // Convert Unity types to ESave's savable types
        gameState.playerData.position = player.transform.position.ToSavable();
        gameState.playerData.rotation = player.transform.rotation.ToSavable();
        gameState.playerData.currentHealth = stats.CurrentHealth;
        gameState.playerData.currentMana = stats.CurrentMana;
    }

    private void RestorePlayerData(GameStateData gameState)
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false;

        // Convert ESave's types back to Unity types
        player.transform.position = _currentGameState.playerData.position.vector3Value;
        player.transform.rotation = _currentGameState.playerData.rotation.quaternionValue;

        controller.enabled = true;
        var stats = player.GetComponent<PlayerStats>();
        stats.RestoreStats(_currentGameState.playerData.currentHealth, _currentGameState.playerData.currentMana);
    }

    private void RestoreObjectiveData(GameStateData gameState)
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RestoreState(_currentGameState.objectiveData);
        }
    }
    private void GatherWorldData(GameStateData gameState)
    {
        var saveableEntities = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<ISaveable>();
        var worldState = new Dictionary<string, object>();

        foreach (var entity in saveableEntities)
        {
            // Add a safety check for the UniqueID
            if (entity != null && !string.IsNullOrEmpty(entity.UniqueID))
            {
                worldState[entity.UniqueID] = entity.CaptureState();
            }
        }

        gameState.worldData = new WorldStateData(worldState);
    }

    private void RestoreWorldData(GameStateData gameState)
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

  
       

    // NEW: A public method to check if a save file exists
    public bool DoesSaveExist(string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveName + ".sav");
        return File.Exists(path);
    }
    
     public string GetLastSaveName()
    {
        // Load the metadata file and read the value
        if (_metaFile.HasData(LastSaveKey))
        {
            return _metaFile.GetData<string>(LastSaveKey);
        }
        return null;
    }

}