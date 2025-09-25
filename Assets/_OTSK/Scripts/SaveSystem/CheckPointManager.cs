using UnityEngine;
using System;

public class CheckpointManager : MonoBehaviour, IResettable
{
    public static CheckpointManager Instance { get; private set; }

    // This will store the position & rotation of the last checkpoint.
    public Vector3 LastCheckpointPosition { get; private set; }
    public Quaternion LastCheckpointRotation { get; private set; }

    public event Action<Transform> OnNewCheckpointSet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       
    }
    #region Save/Load Logic
    public object CaptureState()
    {
        // Create a new data object and populate it with the individual float values.
        return new CheckpointManagerSaveData
        {
            lastCheckpointPosX = LastCheckpointPosition.x,
            lastCheckpointPosY = LastCheckpointPosition.y,
            lastCheckpointPosZ = LastCheckpointPosition.z,
            lastCheckpointRotX = LastCheckpointRotation.x,
            lastCheckpointRotY = LastCheckpointRotation.y,
            lastCheckpointRotZ = LastCheckpointRotation.z,
            lastCheckpointRotW = LastCheckpointRotation.w
        };
    }

    public void RestoreState(object state)
    {
        var saveData = state as CheckpointManagerSaveData;
        if (saveData == null) return;

        // Reconstruct the Vector3 and Quaternion from the saved float values.
        LastCheckpointPosition = new Vector3(saveData.lastCheckpointPosX, saveData.lastCheckpointPosY, saveData.lastCheckpointPosZ);
        LastCheckpointRotation = new Quaternion(saveData.lastCheckpointRotX, saveData.lastCheckpointRotY, saveData.lastCheckpointRotZ, saveData.lastCheckpointRotW);
    }
    #endregion

    // This is the public method that our Checkpoint triggers will call.
    public void SetNewCheckpoint(Transform checkpointTransform)
    {
        LastCheckpointPosition = checkpointTransform.position;
        LastCheckpointRotation = checkpointTransform.rotation;
        Debug.Log($"<color=green>New Checkpoint Set:</color> {checkpointTransform.name}");
        OnNewCheckpointSet?.Invoke(checkpointTransform);
        if (GameManager.Instance != null && GameManager.Instance.CurrentLoadType != GameLoadType.None)
        {
            Debug.Log($"Checkpoint: Suppressing autosave because a load ({GameManager.Instance.CurrentLoadType}) is in progress.");
            return;
        }
        if (SaveLoadManager.Instance != null)
        {
            StartCoroutine(SaveLoadManager.Instance.SaveGame("autosave"));
        }
    }
    public void ResetState()
    {
        LastCheckpointPosition = Vector3.zero;
        LastCheckpointRotation = Quaternion.identity;
    }
}