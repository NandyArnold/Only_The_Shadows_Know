using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UniqueID))]
public class Checkpoint : MonoBehaviour, ISaveable
{
    [System.Serializable]
    public struct CheckpointSaveData
    {
        public bool wasDeactivated;
    }

    private bool _isActive = true;

    private void Awake()
    {
        // Ensure the collider is set to be a trigger.
        GetComponent<Collider>().isTrigger = true;
    }
    private void Start()
    {
        //Debug.Log($"--- Checkpoint.Start() called for {gameObject.name} ---");
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Register(this);
        }
        else
        {
            Debug.LogError($"Could not register {name}, SaveableEntityRegistry.Instance is null!");
        }
    }

    private void OnDestroy() 
    {
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Unregister(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        // When the player enters the trigger...
        if (other.CompareTag("Player"))
        {
            // ...tell the CheckpointManager to set this as the new spawn point.
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetNewCheckpoint(transform);

                // Now, restore the player's health and mana
                if (other.TryGetComponent<PlayerStats>(out var playerStats))
                {
                    playerStats.RestoreHealthToFull();
                    playerStats.RestoreManaToFull();

                    // You can also play a "checkpoint activated" sound/VFX here
                }
                // Optionally, disable this checkpoint so it can't be triggered again.
                _isActive = false;
                gameObject.SetActive(false);
            }
        }
    }

    // --- ISaveable Implementation ---
    public string UniqueID => GetComponent<UniqueID>().ID;
    public object CaptureState()
    {
        // Save if it has been deactivated
        return new CheckpointSaveData
        {
            wasDeactivated = !_isActive
        };
    }
    public void RestoreState(object state)
    {
        // Correctly cast to the CheckpointSaveData struct
        var saveData = (CheckpointSaveData)state;
        if (saveData.wasDeactivated)
        {
            _isActive = false;
            gameObject.SetActive(false);
        }
    }

    // Helper gizmo to see the checkpoint in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f); // Semi-transparent green
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, GetComponent<Collider>().bounds.size);
    }
}