// PatrolState.cs - UPGRADED with Coroutine
using System.Collections;
using UnityEngine;
using System;

public class PatrolState : EnemyAIState
{
    private readonly PatrolRouteSO _patrolRoute;
    private Coroutine _patrolCoroutine;
    private int _currentWaypointIndex = -1;

    public PatrolState(PatrolRouteSO route)
    {
        _patrolRoute = route;
    }

    public override void Enter(EnemyAI enemyAI)
    {
       
        Debug.Log("Entering Patrol State");
        //enemyAI.Navigator.SetSpeed(enemyAI.Config.patrolSpeed);
        //enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);

        // Start the patrol behavior coroutine
        _patrolCoroutine = enemyAI.StartCoroutine(PatrolRoutine(enemyAI));
    }

    public override void Execute(EnemyAI enemyAI)
    {
    
        // The check for the player now transitions to Alert state.
        if (enemyAI.Detector.CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

            if (distanceToPlayer <= enemyAI.Config.combatEntryRange)
            {
                // If player is close, go straight to combat.
                enemyAI.TransitionToState(new CombatState());
            }
            else
            {
                // If player is far, go to alert and investigate.
                enemyAI.LastKnownPlayerPosition = enemyAI.PlayerTarget.position;
                enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
            }
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
            enemyAI.AnimController.SetSpeed(0f);
            Debug.LogWarning("Patrol route is empty, patrol state will do nothing.");
            yield break;
        }

        int currentWaypointIndex = 0;
        while (true)
        {
            PatrolWaypoint waypoint = _patrolRoute.waypoints[currentWaypointIndex];

            // 1. Tell the agent it's time to move again.
            enemyAI.Navigator.Resume();
            enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);

            // Move to the waypoint.
            enemyAI.Navigator.MoveTo(waypoint.position);

            // Wait until we've reached the destination.
            while (Vector3.Distance(enemyAI.transform.position, waypoint.position) > 1.5f)
            {
                yield return null; // Wait for the next frame
            }
            // 2. We've arrived. Tell the agent to stop completely.
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0f); // Enter Idle state


            //yield return new WaitForSeconds(waypoint.waitTime);

            PatrolAction actionToPerform = waypoint.action;

            //enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);   // hmm, maybe this was the problem


            if (waypoint.randomizeAction)
            {
                var allActions = System.Enum.GetValues(typeof(PatrolAction));
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
                    // TODO // Implement the "look around" behavior here.
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