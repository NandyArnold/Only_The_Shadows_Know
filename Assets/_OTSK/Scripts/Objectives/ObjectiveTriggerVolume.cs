// ObjectiveTriggerVolume.cs (CORRECTED)

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveTriggerVolume : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("A unique ID for this trigger volume.")]
    [SerializeField] private string triggerID;

    [Header("Event Channel")]
    [Tooltip("The event to raise when the player enters this volume.")]
    [SerializeField] private StringGameEvent onPlayerEnteredEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered objective trigger volume: {triggerID}");

            if (onPlayerEnteredEvent != null)
            {
                // Raise the ScriptableObject event, passing our ID
                onPlayerEnteredEvent.Raise(triggerID);
            }

            // Deactivate to prevent it from firing multiple times.
            gameObject.SetActive(false);
        }
    }
}