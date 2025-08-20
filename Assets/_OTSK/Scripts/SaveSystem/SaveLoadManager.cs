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
    [SerializeField] private EnemyRegistrySO enemyRegistry;
    [SerializeField] private WeaponRegistrySO weaponRegistry;

    public bool IsLoading { get; private set; }
    private bool _isSceneReadyForRestore = false;

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

    public void SaveGame(string saveFileName)
    {
        
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
        IsLoading = true;
        Debug.Log($"<color=cyan>--- LOAD GAME ROUTINE STARTED ---</color> Attempting to load file: '{saveFileName}'");

        // Use the helper method to get a consistent SaveFile object.
        var saveFile = new SaveFile(GetSetupDataFor(saveFileName));

        if (!saveFile.HasData(GameStateKey))
        {
            Debug.LogError($"<color=red>LOAD FAILED:</color> Save file '{saveFileName}' has no GameState data.", this.gameObject);
            yield break;
        }

        Debug.Log("Found GameState data. Loading...");
        _currentGameState = saveFile.GetData<GameStateData>(GameStateKey);
        // 2. Find the correct SceneDataSO using the saved ID from the registry.
        var sceneDataToLoad = sceneRegistry.GetSceneData(_currentGameState.sceneID);
        if (sceneDataToLoad == null)
        {
            Debug.LogError($"<color=red>LOAD FAILED:</color> Could not find scene with ID '{_currentGameState.sceneID}' in the SceneRegistry!");
            yield break;
        }

        Debug.Log($"Scene to load is '{sceneDataToLoad.SceneName}'. Starting SceneLoader...");
        // 3. Tell the SceneLoader to start loading the scene.
        //    We don't pass a spawn point tag because the save data will override the position.
        SceneLoader.Instance.LoadScene(sceneDataToLoad);
        _isSceneReadyForRestore = false;

        Debug.Log("Waiting for SceneLoader to confirm the new scene is ready...");
        yield return new WaitUntil(() => _isSceneReadyForRestore);
        //Debug.Log("Waiting for player to be spawned...");
        // 4. Wait until the SceneLoader has finished its ENTIRE routine and a new player exists.
        //yield return new WaitUntil(() => GameManager.Instance.Player != null);
        //Debug.Log("<color=green>Player has been spawned!</color> Restoring data...");

        yield return new WaitForEndOfFrame();
        // 5. NOW that the new player exists in the new scene, it's safe to restore all data.
        Debug.Log("<color=green>Scene is ready!</color> Restoring data...");
        RestoreObjectiveData(_currentGameState);
        RestoreWorldData(_currentGameState);
        RestorePlayerData(_currentGameState);

        // 6. Connect the camera to the new player.
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ConnectToPlayer(GameManager.Instance.Player);
        }

        // 7. Revive the player to reset their animation state.
        GameManager.Instance.Player.GetComponent<PlayerStats>().ReviveOnLoad();

        Debug.Log($"<color=cyan>--- GAME LOADED SUCCESSFULLY ---</color> File: '{saveFileName}'");
        IsLoading = false;
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
        Debug.Log($"Restoring world data for scene: {gameState.sceneID}");

        // --- SPAWN & RESTORE DYNAMIC ENEMIES ---
        foreach (var enemySaveEntry in gameState.worldData.enemySaveData) // <-- Make sure this is enemyData, not enemySaveData
        {
            string uniqueId = enemySaveEntry.Key;
            Enemy.EnemySaveData enemyData = enemySaveEntry.Value;

            EnemyConfigSO config = enemyRegistry.GetConfig(enemyData.configSOName);
            if (config == null) continue;

            // --- START OF FINAL FIX ---

            // 1. Prepare the saved position and rotation.
            Vector3 savedPosition = new Vector3(enemyData.posX, enemyData.posY, enemyData.posZ);
            Quaternion savedRotation = new Quaternion(enemyData.rotX, enemyData.rotY, enemyData.rotZ, enemyData.rotW);

            // 2. Instantiate the prefab AT the correct position and rotation from the start.
            GameObject enemyInstance = Instantiate(config.enemyPrefab, savedPosition, savedRotation);

            // 3. Immediately deactivate it.
            enemyInstance.SetActive(false);

            // --- END OF FINAL FIX ---

            // The NavMeshAgent is now created at a valid location. The rest of the logic can proceed.
            var uniqueIdComponent = enemyInstance.GetComponent<UniqueID>();
            uniqueIdComponent.SetID(uniqueId);

            Enemy enemyScript = enemyInstance.GetComponent<Enemy>();
            enemyScript.LoadConfiguration(config);

            enemyScript.RestoreState(enemyData);

            SaveableEntityRegistry.Instance.Register(enemyScript);

            // Activate the GameObject now that it's fully configured.
            enemyInstance.SetActive(true);
            enemyScript.GetComponent<EnemyAI>().StartAI(enemyData.lastWaypointIndex);
            Debug.Log($"<color=lime>Restored spawned enemy:</color> {config.displayName} with saved ID: {uniqueId}");
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

}