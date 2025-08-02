// PatrolRouteSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatrolRoute_", menuName = "Only The Shadows Know/AI/Patrol Route")]
public class PatrolRouteSO : ScriptableObject
{
    [Tooltip("The world-space coordinates for the enemy to patrol between.")]
    public List<Vector3> waypoints;
}