// PatrolState.cs
using UnityEngine;

public class PatrolState : EnemyAIState
{
    private readonly PatrolRouteSO _patrolRoute;
    private int _currentWaypointIndex = 0;

    public PatrolState(PatrolRouteSO route)
    {
        _patrolRoute = route;
    }

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Patrol State");
        enemyAI.Navigator.SetSpeed(enemyAI.Config.patrolSpeed);
        GoToNextWaypoint(enemyAI);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // Check if we've reached the destination
        // Note: NavMeshAgent.remainingDistance can be unreliable. A better check is distance.
        if (Vector3.Distance(enemyAI.transform.position, _patrolRoute.waypoints[_currentWaypointIndex]) < 1.0f)
        {
            GoToNextWaypoint(enemyAI);
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Patrol State");
    }

    private void GoToNextWaypoint(EnemyAI enemyAI)
    {
        if (_patrolRoute.waypoints.Count == 0) return;

        // Move to the current waypoint
        enemyAI.Navigator.MoveTo(_patrolRoute.waypoints[_currentWaypointIndex]);

        // Increment the index for the next time, looping back to 0 if we're at the end.
        _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolRoute.waypoints.Count;
    }
}