// QuestLogUI.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveLogUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject objectiveLogPanelObject;
    [SerializeField] private Transform mainObjectivesContainer;
    [SerializeField] private Transform sideObjectivesContainer;
    [SerializeField] private GameObject objectiveLogEntryPrefab; // Assign your prefab here

  

   

    public void RebuildObjectiveList()
    {
        // Clear old entries
        foreach (Transform child in mainObjectivesContainer) Destroy(child.gameObject);
        foreach (Transform child in sideObjectivesContainer) Destroy(child.gameObject);

        if (ObjectiveManager.Instance == null) return;

        var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();

        foreach (var objectiveInstance in activeObjectives)
        {
            if (objectiveInstance.SourceSO.isHidden) continue;

            Transform parentContainer = null;

            // NOTE: I've renamed your enum cases to match your files, e.g., 'MainObjective'.
            // Please ensure these names match your ObjectiveType enum EXACTLY.
            if (objectiveInstance.SourceSO.objectiveType == ObjectiveType.MainObjective)
            {
                parentContainer = mainObjectivesContainer;
            }
            else if (objectiveInstance.SourceSO.objectiveType == ObjectiveType.SideObjective)
            {
                parentContainer = sideObjectivesContainer;
            }

            if (parentContainer != null)
            {
                var entryObject = Instantiate(objectiveLogEntryPrefab, parentContainer);
                entryObject.GetComponent<ObjectiveLogEntryUI>().Populate(objectiveInstance.SourceSO);
            }
        }
    }
}