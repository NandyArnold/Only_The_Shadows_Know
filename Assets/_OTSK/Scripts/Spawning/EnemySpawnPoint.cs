using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour, ISpawnPoint
{
    [SerializeField]
    [Tooltip("A unique ID for this spawn point (e.g., 'GuardSpawn_A', 'ArcherNest_1').")]
    private string spawnPointID;
    [SerializeField] private bool isReinforcementPoint = false;
    public bool IsReinforcementPoint => isReinforcementPoint;

    // This implements the ISpawnPoint interface
    public string SpawnPointTag => spawnPointID;
    public Transform SpawnTransform => transform;


    private void Start()
    {
        GlobalSpawnRegistry.Instance.RegisterSpawnPoint(this);
      
    }

    private void OnDestroy()
    {
        GlobalSpawnRegistry.Instance.UnregisterSpawnPoint(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}