// Create this new script, ScenePatrolData.cs
using System.Collections.Generic;
using UnityEngine;

public class ScenePatrolData : MonoBehaviour
{
    [Tooltip("A list of all patrol routes that exist in this specific scene.")]
    [SerializeField] private List<PatrolRoute> routesInThisScene;

    private void Start()
    {
        if (PatrolRouteManager.Instance != null)
        {
            foreach (var route in routesInThisScene)
            {
                PatrolRouteManager.Instance.RegisterRoute(route);
            }
            // After registering all routes, signal that the manager is ready for this scene.
            PatrolRouteManager.Instance.SetIsReady(true);
        }
        else
        {
            Debug.LogError("ScenePatrolData could not find PatrolRouteManager.Instance!", this);
        }
    }

    private void OnDisable()
    {
        // When this scene unloads, unregister the routes to keep the manager clean.
        if (PatrolRouteManager.Instance != null)
        {
            foreach (var route in routesInThisScene)
            {
                PatrolRouteManager.Instance.UnregisterRoute(route);
            }
            PatrolRouteManager.Instance.SignalRoutesReady();
        }
    }
}