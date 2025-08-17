// Create this new script, AlarmState.cs
using UnityEngine;
using System.Collections;

public class AlarmState : EnemyAIState
{
    private enum SubState { None, RunningToPanel, SignalingHelp }
    private SubState _subState;

    private AlarmPanel _targetPanel;
    private float _timer;

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Alarm State");

        switch (enemyAI.Config.alarmType)
        {
            case AlarmType.GoToPanel:
                _targetPanel = FindClosestAlarmPanel(enemyAI.transform.position);
                
                if (_targetPanel != null)
                {
                    Debug.Log($" Found alarm panel: {_targetPanel.name}", enemyAI.gameObject);
                    _subState = SubState.RunningToPanel;
                    enemyAI.Navigator.Resume();
                    enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
                    enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
                    enemyAI.Navigator.MoveTo(_targetPanel.transform.position);
                }
                else
                {
                //No panel found.Instead of going to combat, we will
                // transition to the AlertState to investigate the last known position
                // (which HandleDeadBodySpotted correctly set to the body's location).
                Debug.LogWarning("No alarm panel found. Investigating body instead.", enemyAI.gameObject);
                    enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
                }
                break;

            case AlarmType.SignalFromPosition:
                _subState = SubState.SignalingHelp;
                if (enemyAI.SummonCount >= enemyAI.Config.maxSummonCount)
                {
                    enemyAI.TransitionToState(new CombatState());
                    return;
                }
                enemyAI.StartCoroutine(SignalForHelpRoutine(enemyAI));
                break;

            case AlarmType.None:
            default:
                Debug.Log("AlarmType is set to None. Cannot raise an alarm.", enemyAI.gameObject);
                // This enemy can't raise an alarm, so it just goes back to combat.
                enemyAI.TransitionToState(new CombatState());
                break;
        }
       
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (_subState == SubState.RunningToPanel || _subState == SubState.SignalingHelp)
        {
            // The logic for what happens on arrival is still here.
            if (_subState == SubState.RunningToPanel && enemyAI.Navigator.HasReachedDestination)
            {
                enemyAI.Navigator.Stop();
                enemyAI.AnimController.SetSpeed(0);

                if (_targetPanel != null)
                {
                    _targetPanel.TriggerAlarm();
                }
                enemyAI.TransitionToState(new CombatState());
            }
            return; // Exit the method to prevent other checks from running.
        }

        // If we are not busy with an alarm action, the AI is free to check for threats.
        if (enemyAI.PlayerTarget != null && enemyAI.Detector.CanSeePlayer())
        {
            enemyAI.TransitionToState(new CombatState());
        }
    }

    // This coroutine is for the "magical" alarm type
    private IEnumerator SignalForHelpRoutine(EnemyAI enemyAI)
    {
        enemyAI.Navigator.Stop();
        enemyAI.AnimController.SetSpeed(0);
        // TODO: Play a "signaling" or "casting" animation here

        float castTimer = 0f;
        float castDuration = 2f;
        while (castTimer < castDuration)
        {
            if (enemyAI.GetComponent<EnemyHealth>().IsDead)
            {
                yield break; // Exit the coroutine immediately if dead
            }
            castTimer += Time.deltaTime;
            float progress = castTimer / castDuration;
            enemyAI.ReportCastProgress(progress); // Broadcast progress
            yield return null;
        }

        // Hide the cast bar when finished
        enemyAI.ReportCastProgress(0);

        if (enemyAI.Config.alarmEventToRaise != null)
        {
            enemyAI.Config.alarmEventToRaise.Raise();
        }
        enemyAI.IncrementSummonCount();
       

        enemyAI.TransitionToState(new CombatState());
    }


    public override void Exit(EnemyAI enemyAI) { }

    private AlarmPanel FindClosestAlarmPanel(Vector3 position)
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("AlarmPanel");
        AlarmPanel closest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject panel in panels)
        {
            float distance = Vector3.Distance(position, panel.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = panel.GetComponent<AlarmPanel>();
            }
        }
        return closest;
    }
}