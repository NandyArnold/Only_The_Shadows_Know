using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Objective Data")]
    [SerializeField] private LevelObjectiveChainSO _currentLevelObjectiveChain;

    private int _currentObjectiveIndex = 0;
    private List<bool> _objectivesCompleted;

    // Events for the UI to listen to
    public event Action<ObjectiveSO> OnCurrentObjectiveChanged;
    public event Action<ObjectiveSO> OnObjectiveCompleted;
    public event Action OnLevelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// Sets up the manager for a new level's objective chain.
    /// </summary>
    public void Initialize(LevelObjectiveChainSO levelObjectives)
    {
        _currentLevelObjectiveChain = levelObjectives;
        _currentObjectiveIndex = 0;
        _objectivesCompleted = new List<bool>();
        for (int i = 0; i < _currentLevelObjectiveChain.objectives.Count; i++)
        {
            _objectivesCompleted.Add(false);
        }

        // Announce the first objective
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

    /// <summary>
    /// Marks the current objective as complete and advances to the next one.
    /// </summary>
    public void CompleteCurrentObjective()
    {
        ObjectiveSO currentObjective = GetCurrentObjective();
        if (currentObjective == null)
        {
            Debug.LogWarning("Tried to complete objective, but no current objective is active.");
            return;
        }

        // Mark as complete and fire event
        _objectivesCompleted[_currentObjectiveIndex] = true;
        OnObjectiveCompleted?.Invoke(currentObjective);
        Debug.Log($"Objective Completed: {currentObjective.objectiveDescription}");

        _currentObjectiveIndex++;

        // Check if the entire level is complete
        if (_currentObjectiveIndex >= _currentLevelObjectiveChain.objectives.Count)
        {
            OnLevelCompleted?.Invoke();
            Debug.Log($"All objectives for level {_currentLevelObjectiveChain.levelID} completed!");
        }
        else
        {
            // Announce the new current objective
            OnCurrentObjectiveChanged?.Invoke(GetCurrentObjective());
        }
    }
}