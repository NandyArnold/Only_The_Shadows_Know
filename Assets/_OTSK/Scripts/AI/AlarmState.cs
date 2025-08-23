// Create this new script, AlarmState.cs
using UnityEngine;
using System.Collections;

public class AlarmState : EnemyAIState
{
    private enum SubState { None, RunningToPanel, SignalingHelp }
    private SubState _subState;

    private AlarmPanel _targetPanel;
    private float _timer;
    private Coroutine _signalCoroutine;
    // ---It will store the event it needs to raise ---
    private readonly GameEvent _eventToRaise;

    // ---Two constructors to handle both ways of entering the state ---
    public AlarmState() { _eventToRaise = null; } // For GoToPanel type
    public AlarmState(GameEvent eventToRaise) { _eventToRaise = eventToRaise; } // For Signal types
    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Alarm State");

        if (enemyAI.Config.alarmType == AlarmType.GoToPanel)
        {
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
                Debug.LogWarning("Tried to GoToPanel, but no panel was in range. Transitioning to Combat.", enemyAI.gameObject);
                enemyAI.TransitionToState(new CombatState());
            }
        }
        // For SignalFromPosition, we now know exactly which event to use.
        else if (enemyAI.Config.alarmType == AlarmType.SignalFromPosition)
        {
            // --- ADD THIS SAFETY CHECK ---
            if (_eventToRaise == null)
            {
                Debug.LogError($"'{enemyAI.name}' entered AlarmState for a SignalFromPosition alarm, but was not given an event to raise! The alarm will fail. Check the state transition that led here.", enemyAI.gameObject);
                // Fail gracefully by just going to combat instead of getting stuck.
                enemyAI.TransitionToState(new CombatState());
                return;
            }
            // --- END OF CHECK ---

            _subState = SubState.SignalingHelp;
            _signalCoroutine = enemyAI.StartCoroutine(SignalForHelpRoutine(enemyAI));
        }
        else // AlarmType.None
        {
            enemyAI.TransitionToState(new CombatState());
        }

          
        
       
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (_subState == SubState.RunningToPanel && enemyAI.Navigator.HasReachedDestination)
        {
            enemyAI.Navigator.Stop();
            enemyAI.AnimController.SetSpeed(0);
            if (_targetPanel != null)
            {
                _targetPanel.TriggerAlarm();
                enemyAI.IncrementPanelAlarmCount();
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
            enemyAI.ReportCastProgress(castTimer / castDuration);// Broadcast progress
            yield return null;
        }

        // Hide the cast bar when finished
        enemyAI.ReportCastProgress(0);
        if (_eventToRaise != null)
        {
            _eventToRaise.Raise();

            // Increment the correct counter based on which event we just raised
            if (_eventToRaise == enemyAI.Config.summonGameEvent)
            {
                enemyAI.IncrementSummonCount();
            }
            else if (_eventToRaise == enemyAI.Config.instakillGameEvent)
            {
                enemyAI.IncrementInstakillAlarmCount();
            }
        }

        enemyAI.TransitionToState(new CombatState());
    }


    public override void Exit(EnemyAI enemyAI)
    {
        // If the signal coroutine is running when we leave this state, stop it.
        if (_signalCoroutine != null)
        {
            enemyAI.StopCoroutine(_signalCoroutine);
        }
    }

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