// ObjectiveTargetRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTargetRegistry : MonoBehaviour
{
    public static ObjectiveTargetRegistry Instance { get; private set; }

    // A dictionary where the Key is the target's ID (e.g., "scout_enemy") and the
    // Value is a list of all active Transforms with that ID.
    private readonly Dictionary<string, List<Transform>> _targets = new Dictionary<string, List<Transform>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterTarget(string id, Transform targetTransform)
    {
        if (!_targets.ContainsKey(id))
        {
            _targets[id] = new List<Transform>();
        }
        _targets[id].Add(targetTransform);
    }

    public void UnregisterTarget(string id, Transform targetTransform)
    {
        if (_targets.ContainsKey(id))
        {
            _targets[id].Remove(targetTransform);
        }
    }

    // This is the method our other systems will call to find targets.
    public List<Transform> FindTargetsByID(string id)
    {
        _targets.TryGetValue(id, out var foundTargets);
        return foundTargets ?? new List<Transform>(); // Return the list or an empty list
    }
}