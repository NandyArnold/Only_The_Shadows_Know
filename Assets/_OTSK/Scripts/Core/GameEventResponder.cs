// Create this new script, GameEventResponder.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// A helper class to pair an event with a response in the Inspector
[System.Serializable]
public class EventResponse
{
    public GameEvent eventChannel;
    public UnityEvent response;
}

public class GameEventResponder : MonoBehaviour
{
    public List<EventResponse> eventResponses;

    private void OnEnable()
    {
        // Loop through all our event/response pairs and subscribe.
        foreach (var pair in eventResponses)
        {
            if (pair.eventChannel != null)
            {
                pair.eventChannel.OnEventRaised += pair.response.Invoke;
            }
        }
    }

    private void OnDisable()
    {
        // Loop through and unsubscribe to prevent memory leaks.
        foreach (var pair in eventResponses)
        {
            if (pair.eventChannel != null)
            {
                pair.eventChannel.OnEventRaised -= pair.response.Invoke;
            }
        }
    }
}