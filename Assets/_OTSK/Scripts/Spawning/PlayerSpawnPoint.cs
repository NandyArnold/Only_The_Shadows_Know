using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour, ISpawnPoint
{
    [SerializeField] private string spawnPointTag;

    public string SpawnPointTag => spawnPointTag;
    public Transform SpawnTransform => transform;

    private void Awake()
    {
        GlobalSpawnRegistry.Instance?.RegisterSpawnPoint(this);
    }

    private void OnDisable()
    {
        GlobalSpawnRegistry.Instance?.UnregisterSpawnPoint(this);
    }

    // This will draw a helper icon in the Scene view to make the spawn point visible.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}