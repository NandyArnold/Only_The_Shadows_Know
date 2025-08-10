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
    public bool IsAnyEnemyAlerted()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy.GetComponent<EnemyAI>().CurrentState is AlertState)
            {
                return true;
            }
        }
        return false;
    }
    public void ReturnAllToPatrol()
    {
        // We iterate backwards because if an enemy is destroyed, it won't break the loop.
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] != null)
            {
                // Tell their AI to go back to an idle/patrol state
                if (_activeEnemies[i].TryGetComponent<EnemyAI>(out var ai))
                {
                    ai.ForceReturnToPatrol();
                }

                // Also disable their ability to see or hear
                if (_activeEnemies[i].TryGetComponent<DetectionSystem>(out var detector))
                {
                    detector.enabled = false;
                }
            }
        }
    }

    public void ResetAllEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.TryGetComponent<EnemyAI>(out var ai))
            {
                // Re-enable their senses and force them to patrol.
                enemy.Detector.enabled = true;
                ai.ForceReturnToPatrol();
            }
        }
    }
    public void OnPlayerDeath()
    {
        // Tell all enemies to stand down completely.
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                // Disable their ability to see or hear.
                if (enemy.Detector != null)
                {
                    enemy.Detector.enabled = false;
                }

                // Force their AI to a non-aggressive state.
                if (enemy.TryGetComponent<EnemyAI>(out var ai))
                {
                    ai.ForceReturnToPatrol();
                }
            }
        }
    }

    //  Called by the GameManager when the player respawns.
    public void OnPlayerRespawn()
    {
        // Tell all enemies to wake back up.
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.Detector != null)
            {
                // Re-enable their senses.
                enemy.Detector.enabled = true;
            }
        }
    }
}
