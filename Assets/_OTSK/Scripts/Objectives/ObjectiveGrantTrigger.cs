// ObjectiveGrantTrigger.cs (Upgraded Version)
using System.Collections.Generic;
using UnityEngine;

public enum TriggerCondition
{
    OnTriggerEnter,
    OnTriggerExit,
    OnInteraction
}

public class ObjectiveGrantTrigger : MonoBehaviour // Add ", IInteractable" if you have an interaction interface
{
    [Tooltip("How should this trigger be activated?")]
    [SerializeField] private TriggerCondition condition = TriggerCondition.OnTriggerEnter;

    [Tooltip("The side objectives to grant when this is triggered.")]
    [SerializeField] private List<ObjectiveSO> objectivesToGrant;

    [SerializeField] private bool deactivateAfterGranting = true;

    // Called by the player's interaction script
    public void Interact() // This method comes from your IInteractable interface
    {
        if (condition == TriggerCondition.OnInteraction)
        {
            GrantObjectives();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (condition == TriggerCondition.OnTriggerEnter && other.CompareTag("Player"))
        {
            GrantObjectives();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (condition == TriggerCondition.OnTriggerExit && other.CompareTag("Player"))
        {
            GrantObjectives();
        }
    }

    public void GrantObjectives()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AddObjectives(objectivesToGrant);
        }
        if (deactivateAfterGranting)
        {
            gameObject.SetActive(false);
        }
    }
}