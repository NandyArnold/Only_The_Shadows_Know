// The new EnemySpawner.cs
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }


    [Header("Reinforcement Settings")]
    [Tooltip("How close to the player reinforcements should try to spawn.")]
    [SerializeField] private float reinforcementSpawnRadius = 20f;
    [Tooltip("How far from the player to search for pre-placed spawn points.")]
    [SerializeField] private float reinforcementSearchRadius = 40f;

    private SceneDataSO _currentSceneData;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

    }

    private void OnEnable()
    {
        if (SceneLoader.Instance == null) return;
        // Keep this to know WHICH scene to spawn for
        SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoaded;
        // Subscribe to the NEW event to know WHEN to spawn
        SceneLoader.Instance.OnNewSceneReady += SpawnInitialEnemies;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance == null) return;
        SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        SceneLoader.Instance.OnNewSceneReady -= SpawnInitialEnemies;
    }

    // This is called by the SceneLoader when a new scene is ready.
    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        _currentSceneData = sceneData;

    }

    public void SpawnEnemy(SpawnData data)
    {


        var stackTrace = new System.Diagnostics.StackTrace(); string callChain = "CALL STACK:\n";
        // We loop up to 5 frames, or as many as exist in the stack. 
        // We start at i = 1 to skip the current method itself.
        for (int i = 1; i < stackTrace.FrameCount && i < 6; i++)
        {
            var frame = stackTrace.GetFrame(i); var method = frame.GetMethod();
            // Add an arrow and the "ClassName.MethodName" to our string.
            callChain += $"--> {method.DeclaringType.Name}.{method.Name}\n";
        }
        ISpawnPoint spawnPoint = GlobalSpawnRegistry.Instance.GetSpawnPoint(data.spawnPointID);
        //Debug.Log($"--- SPAWN ENEMY CALLED --- \nThis call was received by the spawner on '{gameObject.name}' with ID {GetInstanceID()}.", this.gameObject);
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
                //Debug.Log($"Spawning enemy '{data.enemyToSpawn.displayName}' at spawn point '{spawnPoint.SpawnTransform.name}'" +
                //    $" with patrol route '{routeToAssign?.name ?? "None"}'.");
                enemy.Initialize(data.enemyToSpawn, routeToAssign, data.initialState);
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
                //Debug.LogWarning($"[EnemySPawner] Enemy '{data.enemyToSpawn.displayName}' spawned at world origin due to missing spawn points.");
                enemy.Initialize(data.enemyToSpawn, routeToAssign, data.initialState);
            }
        }
    }
    private void SpawnInitialEnemies()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.IsLoading)
        {
            // If we are loading from a save file, the SaveLoadManager will handle spawning enemies.
            // We skip the default spawn defined in the SceneDataSO.
            Debug.Log("<color=yellow>[EnemySpawner]</color> Skipping initial enemy spawn because a game load is in progress.");
            return;
        }

        if (_currentSceneData == null || _currentSceneData.enemyInitialSpawns == null) return;

        // Loop through each SPAWN GROUP in the list
        foreach (var group in _currentSceneData.enemyInitialSpawns)
        {
            // Then, loop through each individual SPAWN DATA in that group
            foreach (var spawnData in group.spawns)
            {
                SpawnEnemy(spawnData);
            }
        }
    }

    public void TriggerSpawnGroup(string groupName)
    {
        if (_currentSceneData == null || GameManager.Instance.Player == null) return;
        SpawnGroup groupToSpawn = _currentSceneData.enemyEventSpawns.Find(g => g.groupName == groupName);

        if (groupToSpawn != null)
        {
            foreach (var spawnData in groupToSpawn.spawns)
            {
                // Find the best position and rotation for this reinforcement.
                if (FindBestSpawnTransformForAmbush(GameManager.Instance.Player.transform.position, out Vector3 spawnPos, out Quaternion spawnRot))
                {
                    SpawnEnemyAt(spawnData, spawnPos, spawnRot);
                }
                else
                {
                    Debug.LogWarning($"Could not find any valid spawn location for group '{groupName}'.");
                }
            }
        }
    }
    private bool FindBestSpawnTransformForAmbush(Vector3 searchOrigin, out Vector3 position, out Quaternion rotation)
    {
        // First, try to find pre-placed, off-screen spawn points nearby.
        var allSpawnPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);
        var validPoints = allSpawnPoints
         .Where(p => p.IsReinforcementPoint &&
                     Vector3.Distance(searchOrigin, p.transform.position) <= reinforcementSearchRadius &&
                     IsOffScreen(p.transform.position))
         .OrderBy(p => Vector3.Distance(searchOrigin, p.transform.position))
         .ToList();

        if (validPoints.Any())
        {
            position = validPoints.First().transform.position;
            rotation = validPoints.First().transform.rotation;
            return true;
        }

        // --- FALLBACK LOGIC ---
        // If no pre-placed points are found, create a "virtual" one.
        for (int i = 0; i < 10; i++) // Try 10 times to find a random spot
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * reinforcementSpawnRadius;
            Vector3 randomPoint = searchOrigin + new Vector3(randomDir.x, 0, randomDir.y);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                if (IsOffScreen(hit.position))
                {
                    position = hit.position;
                    // Have the enemy spawn facing the player
                    rotation = Quaternion.LookRotation(searchOrigin - hit.position);
                    return true;
                }
            }
        }

        // If all else fails, report failure.
        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }

    private bool IsOffScreen(Vector3 position)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);
        return screenPoint.z < 0 || screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }

    public void SpawnEnemyAt(SpawnData data, Vector3 position, Quaternion rotation)
    {
        PatrolRoute routeToAssign = null;
        if (!string.IsNullOrEmpty(data.patrolRouteID))
        {
            routeToAssign = PatrolRouteManager.Instance.GetRoute(data.patrolRouteID);
        }

        // Instantiate the enemy at the provided position and rotation
        GameObject enemyInstance = Instantiate(data.enemyToSpawn.enemyPrefab, position, rotation);

        enemyInstance.AddComponent<SummonedEnemy>();

        // Perform the rest of the initialization
        SceneManager.MoveGameObjectToScene(enemyInstance, SceneManager.GetActiveScene());
        if (enemyInstance.TryGetComponent<Enemy>(out var enemy))
        {
            Debug.Log($"Spawning reinforcement '{data.enemyToSpawn.displayName}' with patrol route '{routeToAssign?.name ?? "None"}'.");
            enemy.Initialize(data.enemyToSpawn, routeToAssign, data.initialState);
        }
    }



}