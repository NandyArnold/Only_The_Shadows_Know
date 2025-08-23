using UnityEngine;

public class CheckpointManager : MonoBehaviour, IResettable
{
    public static CheckpointManager Instance { get; private set; }

    // This will store the position & rotation of the last checkpoint.
    public Vector3 LastCheckpointPosition { get; private set; }
    public Quaternion LastCheckpointRotation { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       
    }

    // This is the public method that our Checkpoint triggers will call.
    public void SetNewCheckpoint(Transform checkpointTransform)
    {
        LastCheckpointPosition = checkpointTransform.position;
        LastCheckpointRotation = checkpointTransform.rotation;
        Debug.Log($"<color=green>New Checkpoint Set:</color> {checkpointTransform.name}");

        // We can also trigger an autosave here for extra safety.
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame("autosave");
        }
    }
    public void ResetState()
    {
        LastCheckpointPosition = Vector3.zero;
        LastCheckpointRotation = Quaternion.identity;
    }
}