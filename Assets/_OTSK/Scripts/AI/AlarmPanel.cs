// Create this new script, AlarmPanel.cs
using UnityEngine;
using UnityEngine.Events;

public class AlarmPanel : MonoBehaviour
{
    [Tooltip("The events that will fire when this panel is activated.")]
    public UnityEvent OnAlarmTriggered;
    public void TriggerAlarm(GameObject alerter)
    {
        Debug.Log($"<color=red>ALARM TRIGGERED!</color> by {alerter.name}. All enemies should be alerted!");
        OnAlarmTriggered?.Invoke();
        // FUTURE LOGIC:
        // - Play a loud alarm sound that the NoiseManager can broadcast.
        // - Tell the EnemyManager to put all enemies into CombatState.
        // - Tell the EnemySpawner to trigger an "Alarm" spawn group.
    }
}
