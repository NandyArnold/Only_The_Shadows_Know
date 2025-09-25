using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(ScryingIconController))]
[RequireComponent(typeof(RevealableEntity))]
public class Checkpoint : MonoBehaviour
{
    private ScryingIconController _scryingIconController;
    private bool _isCurrentCheckpoint = false;

    private void Awake()
    {
        // Ensure the collider is set to be a trigger.
        GetComponent<Collider>().isTrigger = true;
        _scryingIconController = GetComponent<ScryingIconController>();
    }

    private void OnEnable()
    {
        // Subscribe to the manager's event to know when to update our icon
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnNewCheckpointSet += HandleNewCheckpointSet;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnNewCheckpointSet -= HandleNewCheckpointSet;
        }
    }

    private void Start()
    {
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.LastCheckpointPosition == transform.position)
        {
            _isCurrentCheckpoint = true;
        }
        UpdateIcon();
    }

  
    private void OnTriggerEnter(Collider other)
    {
        if (CombatManager.Instance.IsPlayerInCombat || EnemyManager.Instance.IsAnyEnemyAlerted())
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (CheckpointManager.Instance != null)
            {
                // Set this as the new checkpoint. This will fire the event that all checkpoints listen to.
                CheckpointManager.Instance.SetNewCheckpoint(transform);

                if (other.TryGetComponent<PlayerStats>(out var playerStats))
                {
                    playerStats.RestoreHealthToFull();
                    playerStats.RestoreManaToFull();
                }

           
            }
        }
    }

    private void HandleNewCheckpointSet(Transform newCheckpointTransform)
    {
        // Check if WE are the new checkpoint
        _isCurrentCheckpoint = (newCheckpointTransform == transform);
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (_scryingIconController == null) return;

        // Get the atlas from the ScryingSystem
        var atlas = ScryingSystem.Instance.IconAtlas; // NOTE: You may need to make 'iconAtlas' public in ScryingSystem.cs
        if (atlas == null) return;

        // Determine which type to use based on our state
        RevealableType typeToShow = _isCurrentCheckpoint ? RevealableType.CheckpointCurrent : RevealableType.CheckpointActive;

        // Get the correct sprite and color from the atlas
        Sprite newSprite = atlas.GetIcon(typeToShow);
        Color newColor = atlas.GetColor(typeToShow);

        // Tell the icon to update
        _scryingIconController.ChangeIcon(newSprite, newColor);
    }

    // --- ISaveable Implementation ---
    public string UniqueID => GetComponent<UniqueID>().ID;
    //public object CaptureState()
    //{
    //    // Save if it has been deactivated
    //    return new CheckpointSaveData
    //    {
    //        wasDeactivated = !_isActive
    //    };
    //}
    //public void RestoreState(object state)
    //{
    //    // Correctly cast to the CheckpointSaveData struct
    //    var saveData = (CheckpointSaveData)state;
    //    if (saveData.wasDeactivated)
    //    {
    //        _isActive = false;
    //        gameObject.SetActive(false);
    //    }
    //}

    // Helper gizmo to see the checkpoint in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f); // Semi-transparent green
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, GetComponent<Collider>().bounds.size);
    }
}