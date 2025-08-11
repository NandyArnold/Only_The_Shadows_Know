// In PatrolRouteManager.cs - FINAL VERSION
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatrolRouteManager : MonoBehaviour
{
    public static PatrolRouteManager Instance { get; private set; }

    // It no longer needs a serialized list here.
    private readonly Dictionary<string, PatrolRoute> _patrolRoutes = new Dictionary<string, PatrolRoute>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void RegisterRoute(PatrolRoute route)
    {
        if (route == null || string.IsNullOrEmpty(route.routeID)) return;

        if (!_patrolRoutes.ContainsKey(route.routeID))
        {
            _patrolRoutes.Add(route.routeID, route);
        }
    }

    public void UnregisterRoute(PatrolRoute route)
    {
        if (route != null && _patrolRoutes.ContainsKey(route.routeID))
        {
            _patrolRoutes.Remove(route.routeID);
        }
    }

    public PatrolRoute GetRoute(string id)
    {
        _patrolRoutes.TryGetValue(id, out PatrolRoute route);
        return route;
    }

    public PatrolRoute GetRandomRoute()
    {
        if (_patrolRoutes.Count == 0) return null;
        return _patrolRoutes.Values.ElementAt(Random.Range(0, _patrolRoutes.Count));
    }
}