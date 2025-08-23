// AlertState.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;   

public class AlertState : EnemyAIState
{
    private readonly Vector3 _alertLocation;
    private Coroutine _investigationCoroutine;

    // An enum to track what the state is doing
    private enum SubState { MovingToLocation, Investigating }
    private SubState _subState;


    

    public AlertState(Vector3 alertLocation)
    {
        _alertLocation = alertLocation;
    }

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log($"<color=yellow>[AlertState] ENTER:</color> Investigating location {_alertLocation}", enemyAI.gameObject);
        enemyAI.Navigator.SetStoppingDistance(1.5f);

        // Find a valid and reachable point on the NavMesh.
        if (NavMesh.SamplePosition(_alertLocation, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            if (enemyAI.Navigator.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                // If a path is found, start moving.
                _subState = SubState.MovingToLocation;
                enemyAI.Navigator.Resume();
                enemyAI.Navigator.SetSpeed(enemyAI.Config.alertSpeed);
                enemyAI.AnimController.SetSpeed(enemyAI.Config.alertSpeed);
                enemyAI.Navigator.MoveTo(hit.position);
                return;
            }
        }

        // If no valid path is found, go straight to investigating on the spot.
        _subState = SubState.Investigating;
        
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (!enemyAI.Detector.enabled) return;

        // Always check for a live player or dead body, regardless of sub-state.
        if (enemyAI.PlayerTarget != null && enemyAI.Detector.CanSeePlayer())
        {
            enemyAI.TransitionToState(new CombatState());
            return;
        }
        if (enemyAI.Detector.ScanForDeadBodies(out _))
        {
            enemyAI.TransitionToState(new AlarmState());
            return;
        }

        // If we are moving and have arrived, switch to investigating.
        if (_subState == SubState.MovingToLocation && enemyAI.Navigator.HasReachedDestination)
        {
            _subState = SubState.Investigating;
            _investigationCoroutine = enemyAI.StartCoroutine(InvestigationRoutine(enemyAI));
        }
        else if (_subState == SubState.Investigating && _investigationCoroutine == null)
        {
            // This handles the case where pathing failed in Enter().
            _investigationCoroutine = enemyAI.StartCoroutine(InvestigationRoutine(enemyAI));
        }
    }

    public override void Exit(EnemyAI enemyAI)
    {
        // Stop the investigation coroutine if we are interrupted
        if (_investigationCoroutine != null)
        {
            enemyAI.StopCoroutine(_investigationCoroutine);
        }
        enemyAI.Navigator.SetStoppingDistance(0f);
    }

    // This coroutine handles the "waiting and looking" part.
    private IEnumerator InvestigationRoutine(EnemyAI enemyAI)
    {
        enemyAI.Navigator.Stop();
        enemyAI.AnimController.SetSpeed(0f);

        // 1. Play the "look around" animation.
        enemyAI.AnimController.PlayLookAroundAnimation();

        // 2. Wait for the TOTAL investigation time. The "look around" animation
        //    will play during the beginning of this wait.
        yield return new WaitForSeconds(enemyAI.Config.alertInvestigateTime);

        bool shouldRaiseAlarm = false;
        switch (enemyAI.Config.alarmType)
        {
            case AlarmType.GoToPanel:
                // Check if we are allowed to use panels and haven't hit our limit.
                if (!enemyAI.Config.limitPanelAlarms || enemyAI.PanelAlarmCount < enemyAI.Config.maxPanelAlarms)
                {
                    // Check if there is actually a panel nearby to run to.
                    if (FindClosestAlarmPanel(enemyAI) != null)
                    {
                        shouldRaiseAlarm = true;
                    }
                }
                break;

            case AlarmType.SignalFromPosition:
                // Check if we haven't hit our summon/signal limit.
                if (enemyAI.SummonCount < enemyAI.Config.maxSummonCount)
                {
                    shouldRaiseAlarm = false;
                }
                break;
        }

        if (shouldRaiseAlarm)
        {
            // If any of the conditions were met, escalate to AlarmState.
            enemyAI.LastKnownPlayerPosition = _alertLocation; // Ensure AI knows where the body was
            enemyAI.TransitionToState(new AlarmState());
            yield break; // Stop this coroutine. The new state will take over.
        }

        // --- 3. IF NO ALARM WAS RAISED, RETURN TO NORMAL DUTIES (Original Logic) ---
        if (enemyAI.InitialState == InitialAIState.Guard)
        {
            enemyAI.TransitionToState(new GuardState());
        }
        else
        {
            enemyAI.TransitionToState(new PatrolState(enemyAI.PatrolRoute));
        }
    }

    private AlarmPanel FindClosestAlarmPanel(EnemyAI enemyAI)
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("AlarmPanel");
        AlarmPanel closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject panel in panels)
        {
            float distance = Vector3.Distance(enemyAI.transform.position, panel.transform.position);

            // --- THIS IS THE CHANGE ---
            // We now check if the panel is the closest so far AND within the search radius.
            if (distance < minDistance && distance <= enemyAI.Config.alarmPanelSearchRadius)
            {
                minDistance = distance;
                closest = panel.GetComponent<AlarmPanel>();
            }
        }
        return closest;
    }

}