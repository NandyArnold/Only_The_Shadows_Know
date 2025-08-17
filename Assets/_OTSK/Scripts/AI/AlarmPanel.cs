// Create this new script, AlarmPanel.cs
using UnityEngine;
using UnityEngine.Events;

public class AlarmPanel : MonoBehaviour
{
    [Tooltip("The events that will fire when this panel is activated.")]
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

    public void TriggerAlarm(GameObject alerter)
    {


        Debug.Log($"<color=red>ALARM TRIGGERED!</color> by {name}.");
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
