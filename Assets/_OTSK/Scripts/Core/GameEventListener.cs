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
        response?.Invoke();
    }
}