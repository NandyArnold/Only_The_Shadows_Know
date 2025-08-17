// Create this new script, GameEvent.cs
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent_", menuName = "Only The Shadows Know/Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public event Action OnEventRaised;
    public void Raise() => OnEventRaised?.Invoke();
}