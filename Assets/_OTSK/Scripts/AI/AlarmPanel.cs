// Create this new script, AlarmPanel.cs
using UnityEngine;
using UnityEngine.Events;

public class AlarmPanel : MonoBehaviour
{
    [Header("Alarm Settings")]
    [Tooltip("The radius around this panel in which to alert other enemies.")]
    [SerializeField] private float alertRadius = 25f;
    [Tooltip("The optional Game Event to raise when this alarm is triggered.")]
    [SerializeField] private GameEvent eventToRaiseOnAlarm;

    private Destructible _health;


    private void Awake()
    {
        _health = GetComponent<Destructible>(); // Changed from EnemyHealth
        _health.OnDied += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_health != null) _health.OnDied -= HandleDeath;
    }

    public void TriggerAlarm()
    {


        Debug.Log($"<color=red>ALARM TRIGGERED!</color> by {name}. Alerting allies within {alertRadius}m.");

        // 1. Alert nearby allies directly.
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.AlertAlliesInRange(transform, alertRadius);
        }

        // 2. Raise the GameEvent for other systems (like spawning).
        eventToRaiseOnAlarm?.Raise();
        // Optionally, disable the panel after one use


        this.enabled = false;
        // FUTURE LOGIC:
        // - Play a loud alarm sound that the NoiseManager can broadcast.
        // - Tell the EnemyManager to put all enemies into CombatState.
        // - Tell the EnemySpawner to trigger an "Alarm" spawn group.
    }

    private void HandleDeath()
    {
        Debug.Log("Alarm panel has been destroyed!");
        // You can spawn a "destroyed" VFX here
        gameObject.SetActive(false);
    }
}
