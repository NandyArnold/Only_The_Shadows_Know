// Create this new script, ScenePatrolData.cs
using System.Collections.Generic;
using UnityEngine;

public class ScenePatrolData : MonoBehaviour
{
    [Tooltip("A list of all patrol routes that exist in this specific scene.")]
    [SerializeField] private List<PatrolRoute> routesInThisScene;

    private void OnEnable()
    {
        // When this scene loads, register all of its routes with the central manager.
        if (PatrolRouteManager.Instance != null)
        {
            foreach (var route in routesInThisScene)
            {
                PatrolRouteManager.Instance.RegisterRoute(route);
            }
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
        }
    }
}