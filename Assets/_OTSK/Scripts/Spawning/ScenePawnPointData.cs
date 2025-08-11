// Create this new script, SceneSpawnPointData.cs
using UnityEngine;

public class SceneSpawnPointData : MonoBehaviour
{
    // This script doesn't need any fields. Its only job is to provide a signal.

    private void Start()
    {
        // In Start, we can be sure all spawn points in this scene have run their OnEnable.
        if (GlobalSpawnRegistry.Instance != null)
        {
            Debug.Log("SceneSpawnPointData", this.gameObject);
            GlobalSpawnRegistry.Instance.SetIsReady(true);
        }
    }

    private void OnDisable()
    {
        if (GlobalSpawnRegistry.Instance != null)
        {
            GlobalSpawnRegistry.Instance.SetIsReady(false);
        }
    }
}