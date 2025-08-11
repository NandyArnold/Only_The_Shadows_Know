using System.Collections.Generic;
using UnityEngine;

public class PatrolRouteManager : MonoBehaviour
{
    public static PatrolRouteManager Instance { get; private set; }

    [SerializeField] private List<PatrolRouteSO> patrolRoutes;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public PatrolRouteSO GetRoute(int index)
    {
        if (patrolRoutes == null || index < 0 || index >= patrolRoutes.Count)
        {
            return null;
        }
        return patrolRoutes[index];
    }

    public PatrolRouteSO GetRandomRoute()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            return null;
        }
        return patrolRoutes[Random.Range(0, patrolRoutes.Count)];
    }
}