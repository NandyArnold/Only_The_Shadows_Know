// SaveLoadManager.cs - ESave Powered Version
using Esper.ESave; // Add the ESave namespace
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Esper.ESave.SaveFileSetupData;



public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }


    [Header("Data Registries")] 
    [SerializeField] private SceneRegistrySO sceneRegistry;
    [SerializeField] private EnemyRegistrySO enemyRegistry;
    [SerializeField] private WeaponRegistrySO weaponRegistry;
    [SerializeField] private SkillRegistrySO skillRegistry;

    public bool IsLoading { get; private set; }
    private bool _isSceneReadyForRestore = false;

    private bool _isBusy = false;

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

    private void OnEnable()
    {
        // Subscribe to the event
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnNewSceneReady += HandleNewSceneReady;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnNewSceneReady -= HandleNewSceneReady;
        }
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

    public IEnumerator SaveGame(string saveFileName)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLoadType != GameLoadType.None)
        {
            Debug.LogWarning($"<color=red>SAVE REJECTED:</color> A scene load ({GameManager.Instance.CurrentLoadType}) is in progress. Save request for '{saveFileName}' was ignored.");
            yield break;
        }
        if (_isBusy)
        {
            Debug.LogWarning($"SaveLoadManager is busy. Save request for '{saveFileName}' was ignored.");
            yield break;
        }

        _isBusy = true; // Set the flag
        Debug.Log($"<color=yellow>--- SAVE GAME STARTED ---</color> File: '{saveFileName}'");
        yield return new WaitForEndOfFrame();


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
        Debug.Log($"<color=green>--- GAME SAVED SUCCESSFULLY ---</color> File: {path}");

        _isBusy = false;
    }

    public void LoadGame(string saveFileName)
    {
        if (_isBusy)
        {
            Debug.LogWarning($"SaveLoadManager is busy. Load request for '{saveFileName}' was ignored.");
            return;
        }

        Debug.Log($"<color=cyan>--- LOAD GAME REQUEST RECEIVED ---</color> Attempting to load file: '{saveFileName}'");
        //if (_saveFile == null) return;
        StartCoroutine(LoadGameRoutine(saveFileName));
        
    }

    private IEnumerator LoadGameRoutine(string saveFileName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLoadType(GameLoadType.LoadFromSave);
        }

        _isBusy = true;
        IsLoading = true;
        try
        {
            Debug.Log("SAVELOAD: LoadGameRoutine has started.");
            // --- All of your existing code goes inside this 'try' block ---

            ValidateRegistries();

            var saveFile = new SaveFile(GetSetupDataFor(saveFileName));

            if (!saveFile.HasData(GameStateKey))
            {
                Debug.LogError($"<color=red>LOAD FAILED:</color> Save file '{saveFileName}' has no GameState data.", this.gameObject);
                yield break;
            }

            Debug.Log("Found GameState data. Loading...");
            _currentGameState = saveFile.GetData<GameStateData>(GameStateKey);

            var sceneDataToLoad = sceneRegistry.GetSceneData(_currentGameState.sceneID);
            if (sceneDataToLoad == null)
            {
                Debug.LogError($"<color=red>LOAD FAILED:</color> Could not find scene with ID '{_currentGameState.sceneID}' in the SceneRegistry!");
                yield break;
            }

            Debug.Log($"Scene to load is '{sceneDataToLoad.SceneName}'. Starting SceneLoader...");
            SceneLoader.Instance.LoadScene(sceneDataToLoad);
            _isSceneReadyForRestore = false;

            Debug.Log("SAVELOAD: Waiting for OnNewSceneReady event...");
            yield return new WaitUntil(() => _isSceneReadyForRestore);
            Debug.Log("SAVELOAD: OnNewSceneReady event received.");

            Debug.Log("SAVELOAD: Waiting for PatrolRouteManager to be ready...");
            yield return new WaitUntil(() => PatrolRouteManager.Instance != null && PatrolRouteManager.Instance.IsReady);
            Debug.Log("SAVELOAD: PatrolRouteManager is ready.");

            yield return new WaitForEndOfFrame();

            Debug.Log("<color=green>SAVELOAD: Restoring all game data now.</color>");
            RestoreObjectiveData(_currentGameState);
            RestoreWorldData(_currentGameState);
            RestorePlayerData(_currentGameState);

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.ConnectToPlayer(GameManager.Instance.Player);
            }

            GameManager.Instance.Player.GetComponent<PlayerStats>().ReviveOnLoad();

            Debug.Log($"<color=cyan>--- GAME LOADED SUCCESSFULLY ---</color> File: '{saveFileName}'");
        }
        finally
        {
            // --- This code is GUARANTEED to run ---
            IsLoading = false;
            _isBusy = false;
      
            Debug.Log("<color=yellow>--- LOAD ROUTINE FINISHED ---</color> SaveLoadManager is no longer busy.");
        }
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
        var combat = player.GetComponent<PlayerCombat>();

        // Convert Unity types to ESave's savable types
        gameState.playerData.position = player.transform.position.ToSavable();
        gameState.playerData.rotation = player.transform.rotation.ToSavable();
        gameState.playerData.currentHealth = stats.CurrentHealth;
        gameState.playerData.currentMana = stats.CurrentMana;

        if (combat.CurrentWeapon != null)
        {
            gameState.playerData.equippedWeaponID = combat.CurrentWeapon.name;
        }
    }

    private void RestorePlayerData(GameStateData gameState)
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        var controller = player.GetComponent<CharacterController>();
        var stats = player.GetComponent<PlayerStats>();
        var weaponManager = player.GetComponent<WeaponManager>();
        var playerCombat = player.GetComponent<PlayerCombat>();
        var chargeManager = player.GetComponent<ChargeManager>();

        if (controller != null) controller.enabled = false;

        // Use the 'gameState' parameter
        player.transform.position = gameState.playerData.position.vector3Value;
        player.transform.rotation = gameState.playerData.rotation.quaternionValue;

        if (controller != null) controller.enabled = true;

        if (stats != null)
        {
            stats.RestoreStats(gameState.playerData.currentHealth, gameState.playerData.currentMana);
        }

        if (!string.IsNullOrEmpty(gameState.playerData.equippedWeaponID))
        {
            WeaponSO weaponToEquip = weaponRegistry.GetWeapon(gameState.playerData.equippedWeaponID);
            if (weaponToEquip != null)
            {
                playerCombat.SwitchWeapon(weaponToEquip);
            }
        }
        if (gameState.worldData.chargeManagerSaveData.TryGetValue(chargeManager.UniqueID, out var chargeSaveData))
        {
            chargeManager.RestoreState(chargeSaveData);
            Debug.Log("<color=lime>Player charges restored.</color>");
        }

        //if (weaponManager != null && playerCombat != null && !string.IsNullOrEmpty(gameState.playerData.equippedWeaponID))
        //{
        //    WeaponSO weaponToEquip = weaponRegistry.GetWeapon(gameState.playerData.equippedWeaponID);
        //    if (weaponToEquip != null)
        //    {
        //        // This is the line that was crashing. It should now be safe.
        //        playerCombat.SwitchWeapon(weaponToEquip);
        //    }
        //}
    }

    private void RestoreObjectiveData(GameStateData gameState)
    {
        if (ObjectiveManager.Instance != null)
        {
            // Use the 'gameState' parameter
            ObjectiveManager.Instance.RestoreState(gameState.objectiveData);
        }

    }
    private void GatherWorldData(GameStateData gameState)
    {
        var saveableEntities = SaveableEntityRegistry.Instance.GetSaveableEntities();
        Debug.Log($"<color=yellow>GATHERING WORLD DATA:</color> The SaveableEntityRegistry contains {saveableEntities.Count()} entities.");

        // Clear the lists before populating
        gameState.worldData = new WorldStateData();

        foreach (var entity in saveableEntities)
        {
            if (entity is MonoBehaviour monoBehaviour && monoBehaviour.GetComponent<SummonedEnemy>() != null)
            {
                continue;
            }
            // Sort each entity into its correct category
            if (entity is Enemy enemy)
            {
                gameState.worldData.enemySaveData[enemy.UniqueID] = (Enemy.EnemySaveData)enemy.CaptureState();
            }
            else if (entity is ChargeManager chargeManager)
            {
                gameState.worldData.chargeManagerSaveData[chargeManager.UniqueID] = (ChargeManager.ChargeSaveData)chargeManager.CaptureState();
            }
            else if (entity is Checkpoint checkpoint)
            {
                gameState.worldData.checkpointSaveData[checkpoint.UniqueID] = (Checkpoint.CheckpointSaveData)checkpoint.CaptureState();
            }
        }
    }

    private void RestoreWorldData(GameStateData gameState)
    {
        Debug.Log($"<color=cyan>RESTORING WORLD DATA:</color> The save file contains {gameState.worldData.enemySaveData.Count} enemies.");

        Debug.Log($"Restoring world data for scene: {gameState.sceneID}");
        // --- SPAWN & RESTORE DYNAMIC ENEMIES ---
        foreach (var enemySaveEntry in gameState.worldData.enemySaveData)
        {
            string uniqueId = enemySaveEntry.Key;
            Enemy.EnemySaveData enemyData = enemySaveEntry.Value;

            EnemyConfigSO config = enemyRegistry.GetConfig(enemyData.configSOName);
            if (config == null)
            {
                Debug.LogWarning($"Could not find config '{enemyData.configSOName}' in EnemyRegistry. Skipping enemy.");
                continue;
            }

            // --- THE NEW, ROBUST LOADING SEQUENCE ---

            // 1. Instantiate the prefab AT the correct position and rotation, but keep it inactive.
            Vector3 savedPosition = new Vector3(enemyData.posX, enemyData.posY, enemyData.posZ);
            Quaternion savedRotation = new Quaternion(enemyData.rotX, enemyData.rotY, enemyData.rotZ, enemyData.rotW);
            GameObject enemyInstance = Instantiate(config.enemyPrefab, savedPosition, savedRotation);
            enemyInstance.SetActive(false); // Keep it disabled during setup!

            // 2. Get component references.
            var uniqueIdComponent = enemyInstance.GetComponent<UniqueID>();
            uniqueIdComponent.SetID(uniqueId);
            Enemy enemyScript = enemyInstance.GetComponent<Enemy>();

            // 3. Find the Patrol Route from the saved ID
            PatrolRoute route = null;
            if (!string.IsNullOrEmpty(enemyData.patrolRouteID))
            {
                route = PatrolRouteManager.Instance.GetRoute(enemyData.patrolRouteID);
            }

            // 4. Call our new single setup method. This replaces Awake, Start, and Initialize.
            enemyScript.SetupForLoad(config, route);

            // 5. Restore the variable state (health, etc.).
            enemyScript.RestoreState(enemyData);

            // 6. Register with the save system *after* its ID and state have been set.
            //SaveableEntityRegistry.Instance.Register(enemyScript);

            // 7. NOW, activate the GameObject. This will safely trigger OnEnable.
            enemyInstance.SetActive(true);

            // 8. Finally, tell the fully-initialized enemy to start its AI logic.
            enemyScript.ActivateAIFromLoad(enemyData);

            //Debug.Log($"<color=lime>Restored spawned enemy:</color> {config.displayName} with saved ID: {uniqueId}");
        }

        //foreach (var chargeData in gameState.worldData.chargeManagerSaveData)
        //{
        //    // Find the ChargeManager in the registry by its UniqueID
        //    if (SaveableEntityRegistry.Instance.GetEntity(chargeData.Key) is ChargeManager chargeManager)
        //    {
        //        // Call its RestoreState method
        //        chargeManager.RestoreState(chargeData.Value);
        //        Debug.Log($"<color=lime>Restored charges for:</color> {chargeManager.name}");
        //    }
        //}

        foreach (var checkpointData in gameState.worldData.checkpointSaveData)
        {
            Debug.Log($"Restoring checkpoint: {checkpointData.Key}");
            if (SaveableEntityRegistry.Instance.GetEntity(checkpointData.Key) is Checkpoint checkpoint)
            {
                checkpoint.RestoreState(checkpointData.Value);
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

    private void HandleNewSceneReady()
    {
        // When the scene is truly ready, set our flag
        _isSceneReadyForRestore = true;
        Debug.Log("<color=yellow>SaveLoadManager received OnNewSceneReady event.</color>");
    }

    
    public void DeleteAutosaveAndMeta()
    {
        Debug.Log("<color=red>--- DELETING AUTOSAVE AND META FILES ---</color>");

        // This list now ONLY targets the files relevant to a new game start.
        var filesToDelete = new string[] { "autosave", "meta" };

        foreach (var fileName in filesToDelete)
        {
            if (string.IsNullOrEmpty(fileName)) continue;

            // Use Path.Combine for robust path construction
            string path = Path.Combine(Application.persistentDataPath, SaveSubfolder, fileName + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);

                Debug.Log($"Deleted file: {path}");
            }
        }
    }

    private void ValidateRegistries()
    {
        Debug.Log("<color=yellow>Validating and Resetting Asset Registries...</color>");

        // --- SCENE REGISTRY ---
        if (sceneRegistry == null) // A simpler check is fine now
        {
            Debug.LogWarning("Scene Registry was null. Reloading from Resources.");
            sceneRegistry = Resources.Load<SceneRegistrySO>("SceneRegistrySO");
        }
        //sceneRegistry.Reset(); // Assumes you add Reset() to SceneRegistrySO as well

        // --- ENEMY REGISTRY ---
        if (enemyRegistry == null)
        {
            Debug.LogWarning("Enemy Registry was null. Reloading from Resources.");
            enemyRegistry = Resources.Load<EnemyRegistrySO>("EnemyRegistrySO");
        }
        enemyRegistry.Reset(); // Call the new reset method

        // --- WEAPON REGISTRY ---
        if (weaponRegistry == null)
        {
            Debug.LogWarning("Weapon Registry was null. Reloading from Resources.");
            weaponRegistry = Resources.Load<WeaponRegistrySO>("WeaponRegistrySO");
        }

        if (skillRegistry == null)
        {
            Debug.LogWarning("Skill Registry was null. Reloading from Resources.");
            skillRegistry = Resources.Load<SkillRegistrySO>("SkillRegistry"); // Use the asset's file name
        }
        weaponRegistry.Reset();
    }

    public List<string> GetAllManualSaveNames()
    {
        string saveDirectory = Path.Combine(Application.persistentDataPath, SaveSubfolder);
        if (!Directory.Exists(saveDirectory))
        {
            return new List<string>(); // Return an empty list if the directory doesn't exist
        }

        // Get all .json files, extract their names without the extension,
        // and filter out the "autosave" and "meta" files.
        return Directory.GetFiles(saveDirectory, "*.json")
                        .Select(Path.GetFileNameWithoutExtension)
                        .Where(name => name != "autosave" && name != "meta")
                        .ToList();
    }

}