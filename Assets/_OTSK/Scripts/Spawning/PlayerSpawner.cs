using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    private string _nextSpawnPointTag;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        SceneLoader.Instance.OnSceneLoaded += SpawnPlayer;
    }

    private void OnDisable()
    {
        SceneLoader.Instance.OnSceneLoaded -= SpawnPlayer;
    }

    public void SetNextSpawnPoint(string tag)
    {
        _nextSpawnPointTag = tag;
    }

    private void SpawnPlayer(SceneDataSO sceneData)
    {
        // Don't spawn a player in the Main Menu
        if (sceneData.sceneType == SceneType.Menu) return;

        // Clear previous spawn points before new ones register
        GlobalSpawnRegistry.Instance.ClearRegistry();

        // Use a slight delay to ensure all spawn points in the new scene have registered.
        StartCoroutine(SpawnPlayerRoutine());
    }

    private System.Collections.IEnumerator SpawnPlayerRoutine()
    {
        yield return null; // Wait one frame for spawn points to register in their OnEnable.

        ISpawnPoint spawnPoint = GlobalSpawnRegistry.Instance.GetSpawnPoint(_nextSpawnPointTag);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point '{_nextSpawnPointTag}' not found! Using first available as fallback.");
            spawnPoint = Object.FindAnyObjectByType<PlayerSpawnPoint>(); // Fallback
        }

        if (spawnPoint != null)
        {
            Instantiate(playerPrefab, spawnPoint.SpawnTransform.position, spawnPoint.SpawnTransform.rotation);
        }
        else
        {
            Debug.LogError("No spawn points found in the scene! Spawning player at world origin.");
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        // Clear the tag so it's not reused accidentally
        _nextSpawnPointTag = null;
    }
}