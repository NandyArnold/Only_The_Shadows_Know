using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveCompletionTrigger : MonoBehaviour, IObjectiveTrigger
{
    [SerializeField]
    [Tooltip("This ID must exactly match the 'Completion Trigger ID' on an ObjectiveSO.")]
    private string triggerID;

    public string TriggerID => triggerID;
    public event Action<string> OnTriggerActivated;

    private void OnEnable()
    {
        // Register itself with the manager when it becomes active.
        ObjectiveManager.Instance?.RegisterTrigger(this);
    }

    private void OnDisable()
    {
        // Unregister when it's disabled or destroyed.
        ObjectiveManager.Instance?.UnregisterTrigger(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player.
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered trigger: {triggerID}");
            OnTriggerActivated?.Invoke(triggerID);

            // Deactivate to prevent it from firing multiple times.
            gameObject.SetActive(false);
        }
    }
}