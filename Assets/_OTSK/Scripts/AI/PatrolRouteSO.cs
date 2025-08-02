using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatrolRoute_", menuName = "Only The Shadows Know/AI/Patrol Route")]
public class PatrolRouteSO : ScriptableObject
{
    // UPDATED: This is now a list of our custom PatrolWaypoint class.
    public List<PatrolWaypoint> waypoints;
}