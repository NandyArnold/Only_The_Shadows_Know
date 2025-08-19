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

    //[Header("ESave Configuration")]
    //[SerializeField] private SaveFileSetup saveFileSetup; // Assign this in the Inspector

    private const string SaveSubfolder = "OTSK";

    private SaveFile _metaFile;
    private const string LastSaveKey = "LastSaveName";

    //private SaveFile _saveFile;
    private GameStateData _currentGameState;
    private const string GameStateKey = "GameState"; // The ID for our main data object

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);

        var metaFileSetupData = new SaveFileSetupData()
        {
            fileName = "meta",
            saveLocation = SaveLocation.PersistentDataPath,
            // We can add the subfolder here too for consistency
            filePath = SaveSubfolder,
            addToStorage = false
        };
        _metaFile = new SaveFile(metaFileSetupData);
    }

    private SaveFileSetupData GetSetupDataFor(string fileName)
    {
        return new SaveFileSetupData()
        {
            fileName = fileName,
            saveLocation = SaveLocation.PersistentDataPath,
            filePath = SaveSubfolder // Always use our defined subfolder
        };
    }

    public void SaveGame(string saveFileName)
    {
        // --- THIS IS THE FIX ---
        // Create a new SaveFile instance for the specific file name we are saving to.
        var saveFile = new SaveFile(GetSetupDataFor(saveFileName));

        var currentGameState = new GameStateData();
        currentGameState.playerData = new PlayerStateData();

        if (SceneLoader.Instance.CurrentlyLoadedScene != null)
        {
            currentGameState.sceneID = SceneLoader.Instance.CurrentlyLoadedScene.sceneID;
        }

        GatherPlayerData(currentGameState);
        GatherObjectiveData(currentGameState);
        GatherWorldData(currentGameState);

        saveFile.AddOrUpdateData(GameStateKey, currentGameState);
        saveFile.Save();

        _metaFile.AddOrUpdateData(LastSaveKey, saveFileName);
        _metaFile.Save();

        string path = Path.Combine(Application.persistentDataPath, saveFileName + ".json");
        Debug.Log($"<color=green>Game Saved to:</color> {path} | Last Save is now: {saveFileName}");
    }

    public void LoadGame(string saveFileName)
    {
        Debug.Log($"<color=cyan>--- LOAD GAME REQUEST RECEIVED ---</color> Attempting to load file: '{saveFileName}'");
        //if (_saveFile == null) return;
        StartCoroutine(LoadGameRoutine(saveFileName));
        
    }

    private IEnumerator LoadGameRoutine(string saveFileName)
    {
        Debug.Log($"<color=cyan>--- LOAD GAME ROUTINE STARTED ---</color> Attempting to load file: '{saveFileName}'");

        // Use the new helper method to get a consistent SaveFile object.
        var saveFile = new SaveFile(GetSetupDataFor(saveFileName));

        if (!saveFile.HasData(GameStateKey))
        {
            Debug.LogError($"<color=red>LOAD FAILED:</color> Save file '{saveFileName}' has no GameState data.", this.gameObject);
            yield break;
        }

        Debug.Log("Found GameState data. Loading...");
        var currentGameState = saveFile.GetData<GameStateData>(GameStateKey);
        // 2. Find the correct SceneDataSO using the saved ID from the registry.
        var sceneDataToLoad = sceneRegistry.GetSceneData(_currentGameState.sceneID);
        if (sceneDataToLoad == null)
        {
            Debug.LogError($"<color=red>LOAD FAILED:</color> Could not find scene with ID '{currentGameState.sceneID}' in the SceneRegistry!");
            yield break;
        }

        Debug.Log($"Scene to load is '{sceneDataToLoad.SceneName}'. Starting SceneLoader...");
        // 3. Tell the SceneLoader to start loading the scene.
        //    We don't pass a spawn point tag because the save data will override the position.
        SceneLoader.Instance.LoadScene(sceneDataToLoad);

        Debug.Log("Waiting for player to be spawned...");
        // 4. Wait until the SceneLoader has finished its ENTIRE routine and a new player exists.
        yield return new WaitUntil(() => GameManager.Instance.Player != null);
        Debug.Log("<color=green>Player has been spawned!</color> Restoring data...");

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

        Debug.Log($"<color=cyan>--- GAME LOADED SUCCESSFULLY ---</color> File: '{saveFileName}'");
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
        string path = Path.Combine(Application.persistentDataPath, SaveSubfolder, saveName + ".json");

        bool exists = File.Exists(path);
        Debug.Log($"Checking for save file '{saveName}' at path: {path}. Exists: {exists}");
        return exists;
    }

    public string GetLastSaveName()
    {
        if (_metaFile != null && _metaFile.HasData(LastSaveKey))
        {
            string lastSave = _metaFile.GetData<string>(LastSaveKey);

            // --- NEW DEBUG LOG ---
            Debug.Log($"Found last save name in meta file: '{lastSave}'");
            return lastSave;
        }
        Debug.LogWarning("No last save name found in meta file.");
        return null;
    }

}