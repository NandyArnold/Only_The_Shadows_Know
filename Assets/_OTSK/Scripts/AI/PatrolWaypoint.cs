// Create this new script, PatrolWaypoint.cs
using UnityEngine;

public class PatrolWaypoint : MonoBehaviour
{
    [Tooltip("How long the enemy should wait at this point.")]
    public float waitTime = 2f;

    [Tooltip("The action the enemy should perform at this waypoint.")]
    public PatrolAction action = PatrolAction.Wait;

    [Tooltip("If checked, a random action will be chosen from the list instead.")]
    public bool randomizeAction = false;
}

// You will also need to make sure this enum is in its own file, e.g., PatrolAction.cs

public enum PatrolAction
{
    Wait,
    WaitAndLook,
    Continue
}