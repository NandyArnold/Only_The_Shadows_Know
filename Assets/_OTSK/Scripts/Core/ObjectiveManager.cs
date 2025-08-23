using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour, IResettable
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Objective Data")]
    [SerializeField] private LevelObjectiveChainSO _currentLevelObjectiveChain;

    [Header("Debug Settings")]
    [SerializeField] private bool objectivesEnabled = true;

    private int _currentObjectiveIndex = 0;
    private readonly Dictionary<string, IObjectiveTrigger> _activeTriggers = new Dictionary<string, IObjectiveTrigger>();

    // Events for the UI to listen to
    public event Action<ObjectiveSO> OnCurrentObjectiveChanged;
    public event Action<ObjectiveSO> OnObjectiveCompleted;
    public event Action OnLevelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to the SceneLoader to know when a new scene is ready.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoaded;
        }
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        }
    }
    public void RegisterTrigger(IObjectiveTrigger trigger)
    {
        if (!objectivesEnabled) return;
        if (!_activeTriggers.ContainsKey(trigger.TriggerID))
        {
            _activeTriggers.Add(trigger.TriggerID, trigger);
            trigger.OnTriggerActivated += HandleTriggerActivated;
        }
    }

    public void UnregisterTrigger(IObjectiveTrigger trigger)
    {
        if (!objectivesEnabled) return;
        if (_activeTriggers.ContainsKey(trigger.TriggerID))
        {
            trigger.OnTriggerActivated -= HandleTriggerActivated;
            _activeTriggers.Remove(trigger.TriggerID);
        }
    }

    private void HandleTriggerActivated(string triggerID)
    {
        if (!objectivesEnabled) return;

        ObjectiveSO currentObjective = GetCurrentObjective();
        if (currentObjective != null && currentObjective.completionTriggerID == triggerID)
        {
            CompleteCurrentObjective();
        }
    }

    public void InitializeObjective(LevelObjectiveChainSO levelObjectives)
    {
        if (!objectivesEnabled) return;

        _currentLevelObjectiveChain = levelObjectives;
        _currentObjectiveIndex = 0;

        OnCurrentObjectiveChanged?.Invoke(GetCurrentObjective());
        Debug.Log($"Objective System Initialized for Level: {_currentLevelObjectiveChain.levelID}");
    }

    public ObjectiveSO GetCurrentObjective()
    {
        if (_currentLevelObjectiveChain != null && _currentObjectiveIndex < _currentLevelObjectiveChain.objectives.Count)
        {
            return _currentLevelObjectiveChain.objectives[_currentObjectiveIndex];
        }
        return null;
    }

    public void CompleteCurrentObjective()
    {
        if (!objectivesEnabled || GetCurrentObjective() == null) return;

        Debug.Log($"Objective Completed: {GetCurrentObjective().objectiveDescription}");
        AdvanceToNextObjective();
    }

    // This is now the single method for advancing the objective.
    private void AdvanceToNextObjective()
    {
        _currentObjectiveIndex++;
        if (_currentLevelObjectiveChain != null && _currentObjectiveIndex < _currentLevelObjectiveChain.objectives.Count)
        {
            OnCurrentObjectiveChanged?.Invoke(GetCurrentObjective());
        }
        else
        {
            Debug.Log($"All objectives for level {_currentLevelObjectiveChain.levelID} completed!");
            OnLevelCompleted?.Invoke();
            OnCurrentObjectiveChanged?.Invoke(null);
        }
    }

    public ObjectiveStateData CaptureState()
    {
        return new ObjectiveStateData
        {
            levelID = _currentLevelObjectiveChain.levelID,
            currentObjectiveIndex = _currentObjectiveIndex
        };
    }

    public void RestoreState(ObjectiveStateData state)
    {
        _currentObjectiveIndex = state.currentObjectiveIndex;
        OnCurrentObjectiveChanged?.Invoke(GetCurrentObjective());
    }

    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        if (sceneData.objectiveChain != null)
        {
            _currentLevelObjectiveChain = sceneData.objectiveChain;
            _currentObjectiveIndex = -1; // Reset for the new chain
            AdvanceToNextObjective(); // Start the first objective
        }
        else
        {
            // If the scene has no objectives (like the Main Menu), clear the state.
            _currentLevelObjectiveChain = null;
            _currentObjectiveIndex = -1;
            OnCurrentObjectiveChanged?.Invoke(null);
        }
    }
    public void ResetState()
    {
        // 1. Unsubscribe from any triggers left over from the last scene
        //    to prevent memory leaks.
        foreach (var trigger in _activeTriggers.Values)
        {
            trigger.OnTriggerActivated -= HandleTriggerActivated;
        }
        _activeTriggers.Clear(); // 2. Clear the trigger dictionary

        // 3. Reset the objective progress
        _currentLevelObjectiveChain = null;
        _currentObjectiveIndex = 0;

        // 4. Notify the UI that there is no active objective
        OnCurrentObjectiveChanged?.Invoke(null);
        OnLevelCompleted?.Invoke(); // You might want to call this to reset any "Level Complete" UI too

        Debug.Log("<color=red>ObjectiveManager state has been reset.</color>");
    }

}