using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
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
        if (!objectivesEnabled) return;

        ObjectiveSO currentObjective = GetCurrentObjective();
        if (currentObjective == null) return;

        OnObjectiveCompleted?.Invoke(currentObjective);
        Debug.Log($"Objective Completed: {currentObjective.objectiveDescription}");

        _currentObjectiveIndex++;

        if (_currentObjectiveIndex >= _currentLevelObjectiveChain.objectives.Count)
        {
            OnLevelCompleted?.Invoke();
            Debug.Log($"All objectives for level {_currentLevelObjectiveChain.levelID} completed!");
        }
        else
        {
            OnCurrentObjectiveChanged?.Invoke(GetCurrentObjective());
        }
    }
}