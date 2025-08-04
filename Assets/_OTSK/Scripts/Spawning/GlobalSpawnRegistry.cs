using System.Collections.Generic;
using UnityEngine;

public class GlobalSpawnRegistry : MonoBehaviour
{
    public static GlobalSpawnRegistry Instance { get; private set; }

    private readonly Dictionary<string, ISpawnPoint> _spawnPoints = new Dictionary<string, ISpawnPoint>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterSpawnPoint(ISpawnPoint point)
    {
        if (!_spawnPoints.ContainsKey(point.SpawnPointTag))
        {
            _spawnPoints.Add(point.SpawnPointTag, point);
        }
    }

    public void UnregisterSpawnPoint(ISpawnPoint point)
    {
        if (_spawnPoints.ContainsKey(point.SpawnPointTag))
        {
            _spawnPoints.Remove(point.SpawnPointTag);
        }
    }

    public ISpawnPoint GetSpawnPoint(string tag)
    {
        _spawnPoints.TryGetValue(tag, out ISpawnPoint point);
        return point;
    }

    // Clear the registry when a new level starts loading
    public void ClearRegistry() => _spawnPoints.Clear();
}