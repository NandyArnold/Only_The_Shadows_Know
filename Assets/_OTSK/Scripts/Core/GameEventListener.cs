// Create this new script, GameEventListener.cs
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent eventChannel;
    [SerializeField] private UnityEvent response;

    private void OnEnable()
    {
        if (eventChannel != null) eventChannel.OnEventRaised += OnEventRaised;
    }

    private void OnDisable()
    {
        if (eventChannel != null) eventChannel.OnEventRaised -= OnEventRaised;
    }

    private void OnEventRaised()
    {
        // --- ADD THIS DEBUG LOG ---
        Debug.Log($"<color=green><b>[EVENT HEARD]</b></color> Listener on <b>'{this.name}'</b> heard event: <b>'{eventChannel.name}'</b>. Invoking response.", this.gameObject);
        // --- END OF LOG ---

        response?.Invoke();
    }
}