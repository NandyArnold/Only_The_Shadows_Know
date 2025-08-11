// Create this new script, PatrolRoute.cs
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    // This list will be populated in the Inspector with your waypoint GameObjects
    [Tooltip("A unique ID for this patrol route (e.g., 'GuardPath_A', 'RooftopSniperPath').")]
    public string routeID;
    public List<Transform> waypoints;

    // A helper to draw the path in the Scene view
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                if (i > 0 && waypoints[i - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                }
            }
        }
        // Draw a line back to the start
        if (waypoints[waypoints.Count - 1] != null && waypoints[0] != null)
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
    }
}