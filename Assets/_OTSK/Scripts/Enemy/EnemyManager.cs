// EnemyManager.cs

using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private readonly List<Enemy> _activeEnemies = new List<Enemy>();

    private void Awake()
    {
        //Debug.Log($"--- ENEMY MANAGER INITIALIZED --- \nInstance ID: {GetInstanceID()} on GameObject: {gameObject.name}", this.gameObject);
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!_activeEnemies.Contains(enemy))
        {
            Debug.Log($"Registering enemy: {enemy.gameObject.name} with ID: {enemy.UniqueID}", this.gameObject);
            _activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Remove(enemy);
        }
    }

    public void BroadcastSound(Vector3 sourcePosition, float intensity, GameObject sourceObject)
    {
        foreach (var enemy in _activeEnemies)
        {
            // Don't let enemies hear themselves.
            if (enemy.gameObject == sourceObject) continue;

            // Tell each enemy's detector about the sound.
            enemy.Detector?.OnSoundHeard(sourcePosition, intensity);
        }
    }
}