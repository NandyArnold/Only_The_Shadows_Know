using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// --- NEW SAVE DATA STRUCTURES ---
[Serializable]
public class ObjectiveInstanceSaveData
{
    public string objectiveID;
    public ObjectiveState state;
    public int goalCurrentAmount;
}

[Serializable]
public class ObjectiveStateData
{
    public string levelChainID;
    public List<ObjectiveInstanceSaveData> objectiveStates;
}
// ------------------------------------



public class ObjectiveManager : MonoBehaviour, IResettable
{
    public ObjectiveSO CurrentObjective { get; private set; }

    public bool IsRestoring { get; private set; } = false;
    public static ObjectiveManager Instance { get; private set; }

    [Header("Debug Settings")]
    [SerializeField] private bool objectivesEnabled = true;

    // The live, runtime instances of our objectives for the current level
    private readonly List<ObjectiveInstance> _activeObjectives = new List<ObjectiveInstance>();
    private LevelObjectiveChainSO _currentLevelObjectiveChain;

    // Events for the UI and other systems to listen to
    public event Action<ObjectiveSO> OnCurrentObjectiveChanged; // Fired when a new objective becomes the primary one
    public event Action<ObjectiveSO> OnObjectiveCompleted;     // Fired when any objective is completed
    public event Action OnLevelCompleted;                       // Fired when all objectives in the chain are done


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
        {
            // STAGE 1: Subscribe to this event for DISCOVERY
            SceneLoader.Instance.OnSceneLoadCompleted += PrepareObjectives;

            // STAGE 2: Subscribe to this event for ACTIVATION
            SceneLoader.Instance.OnNewSceneReady += StartObjectiveChain;
        }
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted -= PrepareObjectives;
            SceneLoader.Instance.OnNewSceneReady -= StartObjectiveChain;
        }
        ResetState();
    }

  
    private void PrepareObjectives(SceneDataSO sceneData)
    {
       

        ResetState();

        if (!objectivesEnabled || sceneData.objectiveChain == null)
        {
            //OnCurrentObjectiveChanged?.Invoke(null);
            return;
        }

        _currentLevelObjectiveChain = sceneData.objectiveChain;

        foreach (var objectiveSO in _currentLevelObjectiveChain.objectives)
        {
            _activeObjectives.Add(new ObjectiveInstance(objectiveSO));
        }

        Debug.Log($"[ObjectiveManager] Prepared {_activeObjectives.Count} objectives for level '{_currentLevelObjectiveChain.levelID}'. Waiting for scene to be ready.");
    }


    private void StartObjectiveChain()
    {
        // This method is called after PrepareObjectives.
        // If we are starting a new game, we kick off the first objective.
        // If we are loading from a save, we do NOTHING. We wait for RestoreState to be called.
        if (GameManager.Instance != null && GameManager.Instance.CurrentLoadType != GameLoadType.LoadFromSave)
        {
            Debug.Log("[ObjectiveManager] New game detected. Starting the first objective.");
            ActivateNextObjective();
        }
        else
        {
            Debug.Log("[ObjectiveManager] Load from save detected. Waiting for RestoreState.");
        }
    }


    public void CompleteObjective(ObjectiveInstance completedInstance)
    {
        if (!objectivesEnabled || completedInstance == null) return;

        Debug.Log($"<color=green>Objective Completed:</color> {completedInstance.SourceSO.objectiveDescription}");
        OnObjectiveCompleted?.Invoke(completedInstance.SourceSO);
        foreach (var reward in completedInstance.SourceSO.rewards)
        {
            reward.ExecuteReward();
        }
        ActivateNextObjective();
    }

    private void ActivateNextObjective()
    {
        var nextObjective = _activeObjectives.FirstOrDefault(obj => obj.State == ObjectiveState.Inactive);

        if (nextObjective != null)
        {
            Debug.Log($"<color=cyan>[ObjectiveManager]</color> Activating objective: '{nextObjective.SourceSO.objectiveDescription}'");
            nextObjective.Start();
            CurrentObjective = nextObjective.SourceSO;
            OnCurrentObjectiveChanged?.Invoke(nextObjective.SourceSO);
        }
        else
        {
            if (_currentLevelObjectiveChain != null)
            {
                Debug.Log($"<color=yellow>All objectives for level {_currentLevelObjectiveChain.levelID} completed!</color>");
            }
            OnLevelCompleted?.Invoke();
            CurrentObjective = null;
            OnCurrentObjectiveChanged?.Invoke(null);
        }
    }



    public void ResetState()
    {
        // Tell each active objective to clean itself up (unsubscribe from events)
        foreach (var objective in _activeObjectives)
        {
            objective.CleanUp();
        }
        _activeObjectives.Clear();
        _currentLevelObjectiveChain = null;
    }

    #region Save/Load Logic
    public object CaptureState()
    {
        if (_currentLevelObjectiveChain == null) return null;

        var state = new ObjectiveStateData
        {
            levelChainID = _currentLevelObjectiveChain.levelID,
            objectiveStates = new List<ObjectiveInstanceSaveData>()
        };

        foreach (var instance in _activeObjectives)
        {
            state.objectiveStates.Add(new ObjectiveInstanceSaveData
            {
                objectiveID = instance.SourceSO.objectiveID,
                state = instance.State,
                goalCurrentAmount = instance.Goal.currentAmount
            });
        }
        return state;
    }

    public void RestoreState(object state)
    {

        Debug.Log("<color=orange>--- OBJECTIVE MANAGER: RestoreState CALLED ---</color>");

        var saveData = state as ObjectiveStateData;
        if (saveData == null)
        {
           
            return;
        }

        // We assume the correct scene (and thus correct _currentLevelObjectiveChain) has already been loaded
        if (_currentLevelObjectiveChain == null || _currentLevelObjectiveChain.levelID != saveData.levelChainID)
        {
            Debug.LogError("ObjectiveManager RestoreState failed: Mismatched level chain ID.");
            
            return;
        }

        // Apply the saved state to our newly created instances
        foreach (var savedInstance in saveData.objectiveStates)
        {
            var liveInstance = _activeObjectives.FirstOrDefault(o => o.SourceSO.objectiveID == savedInstance.objectiveID);
            if (liveInstance != null)
            {
                // We don't directly set the state, we reactivate if it was active
                if (savedInstance.state == ObjectiveState.Active)
                {
                    Debug.Log($"<color=orange>--- OBJECTIVE MANAGER: Restoring state for '{liveInstance.SourceSO.objectiveID}'. " +
                        $"Saved state was '{savedInstance.state}'.</color>");
                    liveInstance.Goal.currentAmount = savedInstance.goalCurrentAmount;
                    liveInstance.Start(); // This re-subscribes to events and sets the state to Active
                    //OnCurrentObjectiveChanged?.Invoke(liveInstance.SourceSO);
                }
                else if (savedInstance.state == ObjectiveState.Completed)
                {
                    // Manually set completed objectives
                    liveInstance.MarkCompletedFromLoad();
                    //liveInstance.Goal.currentAmount = liveInstance.Goal.requiredAmount;
                }
            }
        }
        // --- ADD THIS BLOCK: Now we find the correct active objective and force a UI update ---
        var activeObjective = _activeObjectives.FirstOrDefault(o => o.State == ObjectiveState.Active);
        if (activeObjective != null)
        {
            CurrentObjective = activeObjective.SourceSO;
            Debug.Log($"<color=orange>--- OBJECTIVE MANAGER: Restore complete. Broadcasting final active objective:" +
                $" '{CurrentObjective?.objectiveDescription ?? "None"}'</color>");
            OnCurrentObjectiveChanged?.Invoke(CurrentObjective); // Tell the UI about the new objective description

            // Also send the progress update for the counter
            var progressData = GetCurrentProgressData();
            if (progressData.HasValue)
            {
                activeObjective.Goal.onProgressUpdated?.Raise(progressData.Value);
            }
        }
        else
        {
            // If no objective was active (e.g., end of chain), clear the UI
            CurrentObjective = null;
            OnCurrentObjectiveChanged?.Invoke(null);
        }
    }
    #endregion

    public void DEBUG_CompleteCurrentObjective()
    {
        var activeObjective = _activeObjectives.FirstOrDefault(obj => obj.State == ObjectiveState.Active);
        if (activeObjective != null)
        {
            // Manually force the goal to be complete
            activeObjective.Goal.currentAmount = activeObjective.Goal.requiredAmount;
            // Now call the same completion logic as the real system
            CompleteObjective(activeObjective);
        }
        else
        {
            Debug.LogWarning("DEBUG: No active objective to complete.");
        }
    }

    public ObjectiveProgressData? GetCurrentProgressData()
    {
        var activeInstance = _activeObjectives.FirstOrDefault(obj => obj.State == ObjectiveState.Active);
        if (activeInstance != null && activeInstance.Goal != null)
        {
            return new ObjectiveProgressData
            {
                counterLabel = activeInstance.Goal.counterLabel,
                currentProgress = activeInstance.Goal.currentAmount,
                requiredAmount = activeInstance.Goal.requiredAmount
            };
        }
        // Return a nullable struct if no active objective is found
        return null;
    }

    public void SetIsRestoring(bool value)
    {
        IsRestoring = value;
    }

    public void AddObjectives(List<ObjectiveSO> newObjectives)
    {
        foreach (var objectiveSO in newObjectives)
        {
            // Avoid adding duplicates
            if (_activeObjectives.Any(inst => inst.SourceSO == objectiveSO)) continue;

            var newInstance = new ObjectiveInstance(objectiveSO);
            _activeObjectives.Add(newInstance);

            // Start the new objective so it begins listening for its goal.
            newInstance.Start();

            // Announce it to the UI.
            OnCurrentObjectiveChanged?.Invoke(objectiveSO);
        }
    }

}