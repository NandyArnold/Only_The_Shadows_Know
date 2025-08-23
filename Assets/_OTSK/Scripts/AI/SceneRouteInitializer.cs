// SceneRouteInitializer.cs

using UnityEngine;

public class SceneRouteInitializer : MonoBehaviour
{
    // This script's Start() method is guaranteed to run after all Awake() methods are complete.
    private void Start()
    {
        // Find the manager and tell it that all routes in this scene have been registered.
        if (PatrolRouteManager.Instance != null)
        {
            PatrolRouteManager.Instance.SetIsReady(true);
            Debug.Log("<color=cyan>Scene Routes Initialized: PatrolRouteManager is now ready.</color>");
        }
    }
}