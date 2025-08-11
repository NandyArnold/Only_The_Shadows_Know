// The new EnemySpawner.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

  
    [Tooltip("The index of the route from the PatrolRouteManager to assign. Use -1 for a random route.")]
   

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
    }

    private void OnEnable()
    {
        SceneLoader.Instance.OnSceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneLoader.Instance.OnSceneLoaded -= HandleSceneLoaded;
    }

    // This is called by the SceneLoader when a new scene is ready.
    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        // Wait one frame to ensure all spawn points have registered themselves.
        StartCoroutine(SpawnInitialEnemies(sceneData));
    }

    public void SpawnEnemy(SpawnData data)
    {
        ISpawnPoint spawnPoint = GlobalSpawnRegistry.Instance.GetSpawnPoint(data.spawnPointID);
        //Debug.Log($"--- SPAWN PLAYER CALLED --- \nThis call was received by the spawner on '{gameObject.name}' with ID {GetInstanceID()}.", this.gameObject);
        // --- THIS IS THE NEW FALLBACK LOGIC ---
        if (spawnPoint == null)
        {
            Debug.LogWarning($"Enemy spawn point with ID '{data.spawnPointID}' not found! Using first available as fallback.");
            // Find the first available ENEMY spawn point in the scene.
            spawnPoint = Object.FindFirstObjectByType<EnemySpawnPoint>();
        }

        PatrolRoute routeToAssign = null;
        if (!string.IsNullOrEmpty(data.patrolRouteID))
        {
            routeToAssign = PatrolRouteManager.Instance.GetRoute(data.patrolRouteID);
        }


        if (spawnPoint != null)
        {
            // If we found a spawn point (either by ID or fallback), spawn the enemy there.
            GameObject enemyInstance = Instantiate(data.enemyToSpawn.enemyPrefab, spawnPoint.SpawnTransform.position, 
                                        spawnPoint.SpawnTransform.rotation);

            SceneManager.MoveGameObjectToScene(enemyInstance, SceneManager.GetActiveScene());

            if (enemyInstance.TryGetComponent<Enemy>(out var enemy))
            {
                Debug.Log($"Spawning enemy '{data.enemyToSpawn.displayName}' at spawn point '{spawnPoint.SpawnTransform.name}'" +
                    $" with patrol route '{routeToAssign?.name ?? "None"}'.");
                enemy.Initialize(data.enemyToSpawn, routeToAssign);
            }
        }
        else
        {
            // If there are NO enemy spawn points in the entire scene, spawn at the world origin.
            Debug.LogError($"No enemy spawn points found in the scene! Spawning enemy '{data.enemyToSpawn.displayName}'" +
                $" at world origin.");
            GameObject enemyInstance = Instantiate(data.enemyToSpawn.enemyPrefab, Vector3.zero, Quaternion.identity);

            SceneManager.MoveGameObjectToScene(enemyInstance, SceneManager.GetActiveScene());

            if (enemyInstance.TryGetComponent<Enemy>(out var enemy))
            {
                Debug.LogWarning($"[EnemySPawner] Enemy '{data.enemyToSpawn.displayName}' spawned at world origin due to missing spawn points.");
                enemy.Initialize(data.enemyToSpawn, routeToAssign);
            }
        }
    }
    private IEnumerator SpawnInitialEnemies(SceneDataSO sceneData)
    {
        yield return new WaitForEndOfFrame();

        if (sceneData.enemyInitialSpawns == null) yield break;

        foreach (var spawnData in sceneData.enemyInitialSpawns)
        {
            SpawnEnemy(spawnData);
        }
    }

    // This is the core spawning method.
}