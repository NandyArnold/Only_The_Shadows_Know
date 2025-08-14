using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private string _nextSpawnPointTag;

    private void Awake()
    {
        // This is a very verbose singleton check for debugging.
        if (Instance != null && Instance != this)
        {
            //Debug.LogError($"--- DUPLICATE SPAWNER DETECTED --- \nThis spawner on '{gameObject.name}' with ID {GetInstanceID()} is a duplicate. The original is on '{Instance.gameObject.name}'. Destroying this duplicate.", this.gameObject);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        //Debug.LogWarning($"--- SINGLETON INITIALIZED --- \nPlayerSpawner instance is set on '{gameObject.name}' with ID {GetInstanceID()}.", this.gameObject);

    }
    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnNewSceneReady += SpawnPlayer;
        }
    }
    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnNewSceneReady -= SpawnPlayer;
        }
    }
    public void SetNextSpawnPoint(string tag)
    {
        _nextSpawnPointTag = tag;
    }

    private void SpawnPlayer()
    {
        // Failsafe checks
        if (GameManager.Instance.Player != null) return;
        var sceneData = SceneLoader.Instance.CurrentlyLoadedScene;
        if (sceneData.sceneType == SceneType.Menu) return;

        // Find the correct spawn point
        string tagToUse = _nextSpawnPointTag;
        if (string.IsNullOrEmpty(tagToUse))
        {
            tagToUse = sceneData.defaultSpawnPointTag;
        }

        ISpawnPoint spawnPoint = GlobalSpawnRegistry.Instance.GetSpawnPoint(tagToUse);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point '{tagToUse}' not found! Using first available as fallback.");
            spawnPoint = Object.FindFirstObjectByType<PlayerSpawnPoint>();
        }

        // Spawn the player
        if (spawnPoint != null)
        {
            Instantiate(playerPrefab, spawnPoint.SpawnTransform.position, spawnPoint.SpawnTransform.rotation);
        }
        else
        {
            Debug.LogError("No spawn points found in the scene! Spawning player at world origin.");
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        _nextSpawnPointTag = null;
    }
    
}