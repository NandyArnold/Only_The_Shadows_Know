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

    /// <summary>
    /// This is now the DISCOVERY method. It finds and creates objectives but doesn't start them.
    /// </summary>
    private void PrepareObjectives(SceneDataSO sceneData)
    {
        ResetState();

        if (!objectivesEnabled || sceneData.objectiveChain == null)
        {
            OnCurrentObjectiveChanged?.Invoke(null);
            return;
        }

        _currentLevelObjectiveChain = sceneData.objectiveChain;

        foreach (var objectiveSO in _currentLevelObjectiveChain.objectives)
        {
            _activeObjectives.Add(new ObjectiveInstance(objectiveSO));
        }

        Debug.Log($"[ObjectiveManager] Prepared {_activeObjectives.Count} objectives for level '{_currentLevelObjectiveChain.levelID}'. Waiting for scene to be ready.");
    }

    /// <summary>
    /// This is the new ACTIVATION method. It's called later, when the scene is fully ready.
    /// </summary>
    private void StartObjectiveChain()
    {
        if (_activeObjectives.Count > 0)
        {
            Debug.Log("[ObjectiveManager] Scene is ready. Starting the first objective.");
            ActivateNextObjective();
        }
    }



    /// <summary>
    /// Called by an ObjectiveInstance when its goal has been met.
    /// </summary>
    public void CompleteObjective(ObjectiveInstance completedInstance)
    {
        if (!objectivesEnabled || completedInstance == null) return;

        Debug.Log($"<color=green>Objective Completed:</color> {completedInstance.SourceSO.objectiveDescription}");
        OnObjectiveCompleted?.Invoke(completedInstance.SourceSO);

        ActivateNextObjective();
    }

    private void ActivateNextObjective()
    {
        var nextObjective = _activeObjectives.FirstOrDefault(obj => obj.State == ObjectiveState.Inactive);

        if (nextObjective != null)
        {
            Debug.Log($"<color=cyan>[ObjectiveManager]</color> Activating objective: '{nextObjective.SourceSO.objectiveDescription}'");
            nextObjective.Start();
            OnCurrentObjectiveChanged?.Invoke(nextObjective.SourceSO);
        }
        else
        {
            Debug.Log($"<color=yellow>All objectives for level {_currentLevelObjectiveChain.levelID} completed!</color>");
            OnLevelCompleted?.Invoke();
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
        var saveData = state as ObjectiveStateData;
        if (saveData == null) return;

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
                    liveInstance.Goal.currentAmount = savedInstance.goalCurrentAmount;
                    liveInstance.Start(); // This re-subscribes to events and sets the state to Active
                    OnCurrentObjectiveChanged?.Invoke(liveInstance.SourceSO);
                }
                else if (savedInstance.state == ObjectiveState.Completed)
                {
                    // Manually set completed objectives
                    liveInstance.Goal.currentAmount = liveInstance.Goal.requiredAmount;
                }
            }
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
}