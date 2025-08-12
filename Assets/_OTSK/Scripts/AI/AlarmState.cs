// Create this new script, AlarmState.cs
using UnityEngine;

public class AlarmState : EnemyAIState
{
    private enum SubState { FindingPanel, RunningToPanel, ActivatingAlarm }
    private SubState _subState;

    private AlarmPanel _targetPanel;
    private float _timer;

    public override void Enter(EnemyAI enemyAI)
    {
        Debug.Log("Entering Alarm State");

        // 1. Find the closest alarm panel
        _targetPanel = FindClosestAlarmPanel(enemyAI.transform.position);

        if (_targetPanel == null)
        {
            // If there's no alarm panel, just investigate the body instead.
            Debug.LogWarning("No alarm panel found. Investigating body instead.");
            enemyAI.TransitionToState(new AlertState(enemyAI.LastKnownPlayerPosition));
            return;
        }

        // 2. Start running towards it
        _subState = SubState.RunningToPanel;
        enemyAI.Navigator.SetSpeed(enemyAI.Config.chaseSpeed);
        enemyAI.AnimController.SetSpeed(enemyAI.Config.chaseSpeed);
        enemyAI.Navigator.MoveTo(_targetPanel.transform.position);
    }

    public override void Execute(EnemyAI enemyAI)
    {
        if (_subState == SubState.RunningToPanel)
        {
            // If we've reached the panel, switch to the next sub-state
            if (enemyAI.Navigator.HasReachedDestination)
            {
                _subState = SubState.ActivatingAlarm;
                enemyAI.Navigator.Stop();
                enemyAI.AnimController.SetSpeed(0);
                _timer = 2f; // Time it takes to "use" the panel
            }
        }
        else if (_subState == SubState.ActivatingAlarm)
        {
            // Wait for the "use panel" animation/timer
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                // Trigger the alarm and go into combat
                _targetPanel.TriggerAlarm(enemyAI.gameObject);
                enemyAI.TransitionToState(new CombatState());
            }
        }
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