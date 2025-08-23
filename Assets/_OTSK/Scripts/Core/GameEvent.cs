// Create this new script, GameEvent.cs
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent_", menuName = "Only The Shadows Know/Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public event Action OnEventRaised;
    public void Raise()
    {
        // This log will show the name of the ScriptableObject asset that was raised.
        Debug.Log($"<color=purple><b>[EVENT RAISED]</b></color> Game Event asset <b>'{this.name}'</b> was raised.", this);
        OnEventRaised?.Invoke();
    }
}