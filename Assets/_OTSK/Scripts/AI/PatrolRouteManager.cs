// In PatrolRouteManager.cs - FINAL VERSION
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class PatrolRouteManager : MonoBehaviour, IResettable
{
    public static PatrolRouteManager Instance { get; private set; }

    // It no longer needs a serialized list here.
    private readonly Dictionary<string, PatrolRoute> _patrolRoutes = new Dictionary<string, PatrolRoute>();

    public bool IsReady { get; private set; } = false;

    public event Action OnSceneRoutesReady;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        IsReady = false;
    }

    public void SetIsReady(bool isReady)
    {
        IsReady = isReady;
        if (isReady)
        {
            //Debug.Log("<color=cyan>PatrolRouteManager is now READY.</color>");
        }
    }

    public void SignalRoutesReady()
    {
        OnSceneRoutesReady?.Invoke();
    }

    private void OnEnable()
    {
        SceneLoader.Instance.OnSceneLoaded += ClearRoutes;
        SceneLoader.Instance.OnSceneLoaded += (sceneData) => IsReady = false;
    }
    private void OnDisable()
    {
        SceneLoader.Instance.OnSceneLoaded -= ClearRoutes;
        SceneLoader.Instance.OnSceneLoaded -= (sceneData) => IsReady = false;
    }
    public void ClearRoutes(SceneDataSO sceneData)
    {
        _patrolRoutes.Clear();
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
        return _patrolRoutes.Values.ElementAt(UnityEngine.Random.Range(0, _patrolRoutes.Count));
    }

    public void ResetState()
    {
        _patrolRoutes.Clear();
        IsReady = false;
        Debug.Log("<color=orange>PatrolRouteManager state has been reset.</color>");
    }
}