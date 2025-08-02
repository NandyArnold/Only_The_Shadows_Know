// PatrolWaypoint.cs
using UnityEngine;

// Defines the actions an enemy can take at a waypoint.
public enum PatrolAction
{
    Continue,      // Immediately move to the next waypoint.
    Wait,          // Pause at the waypoint for a set duration.
    WaitAndLook    // Pause and perform a "look around" behavior.
}

[System.Serializable] // This makes it show up in the Inspector.
public class PatrolWaypoint
{
    public Vector3 position;
    public PatrolAction action = PatrolAction.Continue;

    [Tooltip("If true, the 'Action' above will be ignored and a random one will be chosen.")]
    public bool randomizeAction = false; // NEW

    [Tooltip("How long to wait at this waypoint (if action is Wait or WaitAndLook).")]
    public float waitTime = 2f;
}