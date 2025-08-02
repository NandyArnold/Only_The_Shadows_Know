// PatrolState.cs - UPGRADED with Coroutine
using System.Collections;
using UnityEngine;
using System;

public class PatrolState : EnemyAIState
{
    private readonly PatrolRouteSO _patrolRoute;
    private Coroutine _patrolCoroutine;

    public PatrolState(PatrolRouteSO route)
    {
        _patrolRoute = route;
    }

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Patrol State");
        enemyAI.Navigator.SetSpeed(enemyAI.Config.patrolSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);

        // Start the patrol behavior coroutine
        _patrolCoroutine = enemyAI.StartCoroutine(PatrolRoutine(enemyAI));
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // The check for the player now transitions to Alert state.
        if (enemyAI.Detector != null && enemyAI.Detector.CanSeePlayer())
        {
            // When we see the player, we become alert and move to their position.
            enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
            return;
        }
        // The Execute method now only needs to check for the player.
        // The coroutine handles all movement and waiting logic.
        if (enemyAI.Detector != null && enemyAI.Detector.CanSeePlayer())
        {
            enemyAI.TransitionToState(new CombatState());
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        Debug.Log("Exiting Patrol State");
        // IMPORTANT: Stop the coroutine when we exit the state.
        if (_patrolCoroutine != null)
        {
            enemyAI.StopCoroutine(_patrolCoroutine);
        }
    }

    private IEnumerator PatrolRoutine(EnemyAI enemyAI)
    {
        if (_patrolRoute == null || _patrolRoute.waypoints.Count == 0)
        {
            Debug.LogWarning("Patrol route is empty, patrol state will do nothing.");
            yield break; // Exit the coroutine if there's no path.
        }

        int currentWaypointIndex = 0;
        while (true)
        {
            PatrolWaypoint waypoint = _patrolRoute.waypoints[currentWaypointIndex];

            // Move to the waypoint.
            enemyAI.Navigator.MoveTo(waypoint.position);
            enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);

            // Wait until we've reached the destination.
            while (Vector3.Distance(enemyAI.transform.position, waypoint.position) > 1.5f)
            {
                yield return null; // Wait for the next frame
            }
            // We've arrived. Tell the animator to STOP before we perform the wait action.
            enemyAI.AnimController.SetSpeed(0f);
            PatrolAction actionToPerform = waypoint.action;


            if (waypoint.randomizeAction)
            {
                // Get all possible enum values and pick a random one.
                var allActions = Enum.GetValues(typeof(PatrolAction));
                actionToPerform = (PatrolAction)allActions.GetValue(UnityEngine.Random.Range(0, allActions.Length));
            }

            switch (actionToPerform)
            {
                case PatrolAction.Wait:
                    Debug.Log("Waiting...");
                    yield return new WaitForSeconds(waypoint.waitTime);
                    break;
                case PatrolAction.WaitAndLook:
                    Debug.Log("Waiting and Looking...");
                    yield return new WaitForSeconds(waypoint.waitTime);
                    break;
                case PatrolAction.Continue:
                    Debug.Log("Continuing...");
                    break;
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % _patrolRoute.waypoints.Count;
        }
    }
}