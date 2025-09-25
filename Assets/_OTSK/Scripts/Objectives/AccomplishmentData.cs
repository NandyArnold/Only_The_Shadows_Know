// AccomplishmentTracker.cs
using System.Collections.Generic;
using UnityEngine;



public class AccomplishmentTracker : MonoBehaviour, IResettable
{
    public static AccomplishmentTracker Instance { get; private set; }

    [Header("Event Channels to Track")]
    [SerializeField] private StringGameEvent onEnemyDiedEventChannel;
    [SerializeField] private StringGameEvent onDestroyedEventChannel;
    [SerializeField] private StringGameEvent onTriggerEnteredEventChannel;

    private AccomplishmentData _accomplishments = new AccomplishmentData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        

        // Subscribe to events immediately to start tracking from the moment the game starts
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (onEnemyDiedEventChannel != null) onEnemyDiedEventChannel.OnEventRaised += HandleEnemyDied;
        if (onDestroyedEventChannel != null) onDestroyedEventChannel.OnEventRaised += HandleDestructibleDestroyed;
        if (onTriggerEnteredEventChannel != null) onTriggerEnteredEventChannel.OnEventRaised += HandleLocationVisited;
    }

    private void UnsubscribeFromEvents()
    {
        if (onEnemyDiedEventChannel != null) onEnemyDiedEventChannel.OnEventRaised -= HandleEnemyDied;
        if (onDestroyedEventChannel != null) onDestroyedEventChannel.OnEventRaised -= HandleDestructibleDestroyed;
        if (onTriggerEnteredEventChannel != null) onTriggerEnteredEventChannel.OnEventRaised -= HandleLocationVisited;
    }

    // --- Event Handlers ---
    private void HandleEnemyDied(string enemyID)
    {
        if (!_accomplishments.killCounts.ContainsKey(enemyID))
        {
            _accomplishments.killCounts[enemyID] = 0;
        }
        _accomplishments.killCounts[enemyID]++;
        Debug.Log($"[AccomplishmentTracker] Logged kill: {enemyID}. New count: {_accomplishments.killCounts[enemyID]}");
    }

    private void HandleDestructibleDestroyed(string destructibleID)
    {
        if (!_accomplishments.destroyCounts.ContainsKey(destructibleID))
        {
            _accomplishments.destroyCounts[destructibleID] = 0;
        }
        _accomplishments.destroyCounts[destructibleID]++;
    }

    private void HandleLocationVisited(string locationID)
    {
        _accomplishments.visitedLocationIDs.Add(locationID);
        Debug.Log($"[AccomplishmentTracker] Logged visit to: {locationID}.");
    }

    // --- Public API for Goals to Query ---
    public int GetKillCount(string enemyID)
    {
        return _accomplishments.killCounts.TryGetValue(enemyID, out int count) ? count : 0;
    }

    public int GetDestroyCount(string destructibleID)
    {
        return _accomplishments.destroyCounts.TryGetValue(destructibleID, out int count) ? count : 0;
    }

    public bool HasVisitedLocation(string locationID)
    {
        return _accomplishments.visitedLocationIDs.Contains(locationID);
    }

    // --- Save/Load Integration ---
    public object CaptureState()
    {
        return _accomplishments;
    }

    public void RestoreState(object state)
    {
        _accomplishments = state as AccomplishmentData;
        if (_accomplishments == null)
        {
            // If there's no save data, create a fresh instance
            _accomplishments = new AccomplishmentData();
        }
    }

    public void ResetState()
    {
        // To reset, we just create a new, empty data object.
        _accomplishments = new AccomplishmentData();
        Debug.Log("[AccomplishmentTracker] State has been reset.");
    }
}