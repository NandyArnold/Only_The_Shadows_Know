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
                    _subState = SubState.RunningToPanel;
                    enemyAI.Navigator.Resume();
                    enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
                    enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
                    enemyAI.Navigator.MoveTo(_targetPanel.transform.position);
                }
                else
                {
                    // No panel found, go back to combat to stay aggressive.
                    Debug.LogWarning("No alarm panel found. Returning to combat.", enemyAI.gameObject);
                    enemyAI.TransitionToState(new CombatState());
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
                // This enemy can't raise an alarm, so it just goes back to combat.
                enemyAI.TransitionToState(new CombatState());
                break;
        }
       
    }

    public override void Execute(EnemyAI enemyAI)
    {
        // This logic is only for the "run to panel" type
        if (_subState == SubState.RunningToPanel && enemyAI.Navigator.HasReachedDestination)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0);

            // Trigger the alarm and immediately go back to combat to defend the panel
            if (_targetPanel != null)
            {
                _targetPanel.TriggerAlarm();
            }
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