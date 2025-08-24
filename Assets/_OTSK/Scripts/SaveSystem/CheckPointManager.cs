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