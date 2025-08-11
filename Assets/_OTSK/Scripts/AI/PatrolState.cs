// PatrolState.cs - UPGRADED with Coroutine
using System.Collections;
using UnityEngine;
using System;

public class PatrolState : EnemyAIState
{
    private  PatrolRoute _patrolRoute;
    private Coroutine _patrolCoroutine;
    private int _currentWaypointIndex = -1;
    private float _gracePeriodTimer;



    public PatrolState(PatrolRoute route) // Change type from PatrolRouteSO
    {
        _patrolRoute = route;
    }

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Patrol State");
        _gracePeriodTimer = 0.5f;

        _patrolCoroutine = enemyAI.StartCoroutine(PatrolRoutine(enemyAI));
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if(_patrolRoute == null )
        {
            Debug.LogWarning("Patrol route is null, cannot execute patrol state.");
        }
        if (_gracePeriodTimer > 0)
        {
            _gracePeriodTimer -= Time.deltaTime;
            return; // Do nothing else until the grace period is over.
        }
        // The check for the player now transitions to Alert state.
        if (enemyAI.PlayerTarget != null && enemyAI.Detector.CanSeePlayer())
        {
            
            float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.PlayerTarget.position);

            if (distanceToPlayer <= enemyAI.Config.combatEntryRange && enemyAI.PlayerTarget != null)
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

        int _currentWaypointIndex = -1;
        while (true)
        {
            while (true)
            {
                // 1. CHOOSE NEXT WAYPOINT
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolRoute.waypoints.Count;
                Transform waypointTransform = _patrolRoute.waypoints[_currentWaypointIndex];


                // 2. START MOVING
                enemyAI.Navigator.Resume();
                enemyAI.AnimController.SetSpeed(enemyAI.Config.patrolSpeed);
                enemyAI.Navigator.MoveTo(waypoint.position);

                // 3. WAIT TO ARRIVE (Using the reliable check)
                yield return new WaitUntil(() => enemyAI.Navigator.HasReachedDestination);

                // 4. ARRIVED: STOP AND WAIT
                enemyAI.Navigator.Stop();
                enemyAI.AnimController.SetSpeed(0f); // Enter Idle state

                PatrolAction actionToPerform = waypoint.action;
                if (waypoint.randomizeAction)
                {
                    var allActions = System.Enum.GetValues(typeof(PatrolAction));
                    actionToPerform = (PatrolAction)allActions.GetValue(UnityEngine.Random.Range(0, allActions.Length));
                }

                switch (actionToPerform)
                {
                    case PatrolAction.Wait:
                        yield return new WaitForSeconds(waypoint.waitTime);
                        break;
                    case PatrolAction.WaitAndLook:
                        // TODO: Implement look behavior
                        yield return new WaitForSeconds(waypoint.waitTime);
                        break;
                    case PatrolAction.Continue:
                        // Do nothing, will immediately loop to the next waypoint
                        break;
                }
            }
        }
    }
}