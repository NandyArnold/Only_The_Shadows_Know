// PatrolState.cs - CORRECTED VERSION

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

        // Set the very first destination immediately.
        if (_patrolRoute != null && _patrolRoute.waypoints.Count > 0)
        {
            enemyAI.Navigator.MoveTo(_patrolRoute.waypoints[0]);
        }
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // First, always check if we can see the player.
        if (enemyAI.Detector != null && enemyAI.Detector.CanSeePlayer())
        {
            enemyAI.TransitionToState(new ChaseState());
            return;
        }

        if (_patrolRoute == null || _patrolRoute.waypoints.Count == 0) return;

        // Check if we are close enough to our CURRENT target waypoint.
        if (Vector3.Distance(enemyAI.transform.position, _patrolRoute.waypoints[_currentWaypointIndex]) < 1.5f)
        {
            // If we are, increment the index to get the next waypoint.
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolRoute.waypoints.Count;
            // Set the new destination.
            enemyAI.Navigator.MoveTo(_patrolRoute.waypoints[_currentWaypointIndex]);
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Patrol State");
    }
}