// ObjectiveRegistrySO.cs (UPGRADED with Structured Lists)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This new class defines the structure for a single level's entry in our registry.
[System.Serializable]
public class LevelObjectiveEntry
{
    public string levelID; // e.g., "Tutorial", "Mission01"
    [Tooltip("The main, linear quest objectives for this level.")]
    public List<ObjectiveSO> mainObjectives;
    [Tooltip("Optional side quests available in this level.")]
    public List<ObjectiveSO> sideObjectives;
    [Tooltip("Optional hidden objectives available in this level.")]
    public List<ObjectiveSO> hiddenObjectives;
}

[CreateAssetMenu(fileName = "ObjectiveRegistry", menuName = "Only The Shadows Know/Objectives/Objective Registry")]
public class ObjectiveRegistrySO : ScriptableObject
{
    [SerializeField] private List<LevelObjectiveEntry> levelEntries;

    private Dictionary<string, ObjectiveSO> _objectiveIdMap;

    public void Initialize()
    {
        // We now need to build the dictionary from our new nested structure.
        _objectiveIdMap = new Dictionary<string, ObjectiveSO>();

        foreach (var entry in levelEntries)
        {
            // Add all objectives from each list to the master dictionary.
            foreach (var objective in entry.mainObjectives) { _objectiveIdMap[objective.objectiveID] = objective; }
            foreach (var objective in entry.sideObjectives) { _objectiveIdMap[objective.objectiveID] = objective; }
            foreach (var objective in entry.hiddenObjectives) { _objectiveIdMap[objective.objectiveID] = objective; }
        }

        Debug.Log($"<color=green>Objective Registry Initialized with {_objectiveIdMap.Count} total objectives across {levelEntries.Count} levels.</color>");
    }

    public ObjectiveSO GetObjectiveByID(string id)
    {
        _objectiveIdMap.TryGetValue(id, out var objective);
        if (objective == null)
        {
            Debug.LogWarning($"Could not find objective with ID '{id}' in the registry.");
        }
        return objective;
    }
}