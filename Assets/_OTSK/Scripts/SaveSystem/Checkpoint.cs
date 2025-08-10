using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    private void Awake()
    {
        // Ensure the collider is set to be a trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // When the player enters the trigger...
        if (other.CompareTag("Player"))
        {
            // ...tell the CheckpointManager to set this as the new spawn point.
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetNewCheckpoint(transform);
                // Optionally, disable this checkpoint so it can't be triggered again.
                gameObject.SetActive(false);
            }
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