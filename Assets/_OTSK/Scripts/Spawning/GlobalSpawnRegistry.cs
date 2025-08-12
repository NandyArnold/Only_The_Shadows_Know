using System.Collections.Generic;
using UnityEngine;

public class GlobalSpawnRegistry : MonoBehaviour
{
    public static GlobalSpawnRegistry Instance { get; private set; }

    public bool IsReady { get; private set; } = false;


    private readonly Dictionary<string, ISpawnPoint> _spawnPoints = new Dictionary<string, ISpawnPoint>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Debug.LogWarning("Duplicate GlobalSpawnRegistry found. Destroying...", this.gameObject);
            Destroy(this.gameObject);
            return;
        }
        else
            Instance = this;
    }

    private void OnEnable()
    {
        SceneLoader.Instance.OnSceneLoaded += (sceneData) => IsReady = true;
    }

    private void OnDisable()
    {
        SceneLoader.Instance.OnSceneLoaded -= (sceneData) => IsReady = true;
    }

    public void SetIsReady(bool isReady)
    {
        IsReady = isReady;
        if (isReady)
        {
            Debug.Log("<color=cyan>GlobalSpawnRegistry is now READY.</color>");
        }
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