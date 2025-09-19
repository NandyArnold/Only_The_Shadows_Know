using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// --- NEW SAVE DATA STRUCTURES ---
//[Serializable]
//public class ObjectiveInstanceSaveData
//{
//    public string objectiveID;
//    public ObjectiveState state;
//    public int goalCurrentAmount;
//}

//[Serializable]
//public class ObjectiveStateData
//{
//    public string levelChainID;
//    public List<ObjectiveInstanceSaveData> objectiveStates;
//}
//// ------------------------------------
//// Add this to your save data structures, likely near the top of ObjectiveManager.cs
//[System.Serializable]
//public class AccomplishmentData
//{
//    public Dictionary<string, int> killCounts = new Dictionary<string, int>();
//    public Dictionary<string, int> destroyCounts = new Dictionary<string, int>();
//    public HashSet<string> visitedLocationIDs = new HashSet<string>();
//}



public class ObjectiveManager : MonoBehaviour, IResettable
{


    public ObjectiveSO CurrentObjective { get; private set; }

    public bool IsRestoring { get; private set; } = false;
    public static ObjectiveManager Instance { get; private set; }

    [Header("Data")] 
    [SerializeField] private ObjectiveRegistrySO objectiveRegistry;

    [Header("Debug Settings")]
    [SerializeField] private bool objectivesEnabled = true;

    // The live, runtime instances of our objectives for the current level
    private readonly List<ObjectiveInstance> _activeObjectives = new List<ObjectiveInstance>();
    private LevelObjectiveChainSO _currentLevelObjectiveChain;

    // Events for the UI and other systems to listen to
    public event Action<ObjectiveSO> OnCurrentObjectiveChanged; // Fired when a new objective becomes the primary one
    public event Action<ObjectiveSO> OnObjectiveCompleted;     // Fired when any objective is completed
    public event Action OnLevelCompleted;                       // Fired when all objectives in the chain are done
    public event Action<ObjectiveSO> OnSideObjectiveAdded;


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        objectiveRegistry.Initialize();
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

        if (completedInstance.State == ObjectiveState.Completed)
        {
            return;
        }
        completedInstance.MarkCompletedFromLoad();


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
            // First, check if the objective's conditions have already been met.
            if (nextObjective.Goal.IsAlreadyCompleted())
            {
                Debug.Log($"<color=#87CEEB>[ObjectiveManager]</color> Objective '{nextObjective.SourceSO.objectiveTitle}' was already completed. Completing retroactively.");
                // This immediately completes it and will trigger this method again,
                // effectively skipping to the next available objective.
                CompleteObjective(nextObjective);
                return; // Stop here to let the recursive call handle the next step.
            }
            else
            {
                // If not already completed, proceed with normal activation.
                Debug.Log($"<color=cyan>[ObjectiveManager]</color> Activating objective: '{nextObjective.SourceSO.objectiveDescription}'");
                nextObjective.Start();
                CurrentObjective = nextObjective.SourceSO;
                OnCurrentObjectiveChanged?.Invoke(nextObjective.SourceSO);
                nextObjective.NotifyUIOfInitialState(); // (Assuming you kept this from our previous fix)
            }
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
        var saveData = state as ObjectiveStateData;
        if (saveData == null) return;

        // In this new version, we don't need to check the level chain ID.
        // We will build the objective list directly from the save data.
        ResetState(); // Start with a completely clean slate.

        foreach (var savedInstance in saveData.objectiveStates)
        {
            // Use the registry to find the original ObjectiveSO asset from its ID.
            ObjectiveSO so = objectiveRegistry.GetObjectiveByID(savedInstance.objectiveID);
            if (so == null)
            {
                Debug.LogWarning($"Could not find objective with ID '{savedInstance.objectiveID}' in the registry. Skipping.");
                continue;
            }

            // Create a new instance for this objective and add it to our list.
            var liveInstance = new ObjectiveInstance(so);
            _activeObjectives.Add(liveInstance);

            // Now, apply the saved state to this newly created instance.
            if (savedInstance.state == ObjectiveState.Active || savedInstance.state == ObjectiveState.Completed)
            {
                liveInstance.Goal.currentAmount = savedInstance.goalCurrentAmount;
            }
            if (savedInstance.state == ObjectiveState.Active)
            {
                liveInstance.Start();
            }
            else if (savedInstance.state == ObjectiveState.Completed)
            {
                liveInstance.MarkCompletedFromLoad();
            }
        }

        var activeObjective = _activeObjectives.FirstOrDefault(o => o.State == ObjectiveState.Active && o.SourceSO.objectiveType == ObjectiveType.MainObjective);
        if (activeObjective == null)
        {
            // If no main quest is active, find any active side quest to display.
            activeObjective = _activeObjectives.FirstOrDefault(o => o.State == ObjectiveState.Active);
        }

        if (activeObjective != null)
        {
            CurrentObjective = activeObjective.SourceSO;
            OnCurrentObjectiveChanged?.Invoke(CurrentObjective);
        }
        else
        {
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
                
                objectiveID = activeInstance.SourceSO.objectiveID,

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
            //OnCurrentObjectiveChanged?.Invoke(objectiveSO);
            OnSideObjectiveAdded?.Invoke(objectiveSO);
        }
    }

    public IEnumerable<ObjectiveInstance> GetActiveObjectives()
    {
        // We only return objectives that aren't completed yet
        return _activeObjectives.Where(inst => inst.State == ObjectiveState.Active);
    }

    public IEnumerable<ObjectiveInstance> GetAllCurrentObjectives()
    {
        return _activeObjectives;
    }

}