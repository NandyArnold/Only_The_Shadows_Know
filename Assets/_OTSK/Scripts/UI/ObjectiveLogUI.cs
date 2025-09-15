// QuestLogUI.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectiveLogUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject objectiveLogPanelObject;
    [SerializeField] private Transform mainObjectivesContainer;
    [SerializeField] private Transform sideObjectivesContainer;
    [SerializeField] private GameObject objectiveLogEntryPrefab; 





    public void RebuildObjectiveList()
    {
        // Clear old entries
        foreach (Transform child in mainObjectivesContainer) Destroy(child.gameObject);
        foreach (Transform child in sideObjectivesContainer) Destroy(child.gameObject);

        if (ObjectiveManager.Instance == null) return;

        // Get ALL objectives from the manager
        var allObjectives = ObjectiveManager.Instance.GetAllCurrentObjectives();

        // --- NEW SORTING LOGIC ---
        // Find the one active main quest
        var activeMainQuest = allObjectives.FirstOrDefault(obj =>
            obj.SourceSO.objectiveType == ObjectiveType.MainObjective &&
            obj.State == ObjectiveState.Active);

        // Find all completed main quests
        var completedMainQuests = allObjectives.Where(obj =>
            obj.SourceSO.objectiveType == ObjectiveType.MainObjective &&
            obj.State == ObjectiveState.Completed);

        // Find all active side quests
        var activeSideQuests = allObjectives.Where(obj =>
            obj.SourceSO.objectiveType == ObjectiveType.SideObjective &&
            obj.State == ObjectiveState.Active);
        // -------------------------

        // 1. Display the current active main quest at the top.
        if (activeMainQuest != null)
        {
            var entryObject = Instantiate(objectiveLogEntryPrefab, mainObjectivesContainer);
            // We now pass the entire instance to Populate
            entryObject.GetComponent<ObjectiveLogEntryUI>().Populate(activeMainQuest);
        }

        // 2. Display all completed main quests below it.
        foreach (var objectiveInstance in completedMainQuests)
        {
            var entryObject = Instantiate(objectiveLogEntryPrefab, mainObjectivesContainer);
            entryObject.GetComponent<ObjectiveLogEntryUI>().Populate(objectiveInstance);
        }

        // 3. Display all active side quests in their own section.
        foreach (var objectiveInstance in activeSideQuests)
        {
            var entryObject = Instantiate(objectiveLogEntryPrefab, sideObjectivesContainer);
            entryObject.GetComponent<ObjectiveLogEntryUI>().Populate(objectiveInstance);
        }
    }
}