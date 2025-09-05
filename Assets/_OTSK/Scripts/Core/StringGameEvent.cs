// StringGameEvent.cs

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StringEvent_", menuName = "Only The Shadows Know/Events/String Game Event")]
public class StringGameEvent : ScriptableObject
{
    public event Action<string> OnEventRaised;

    public void Raise(string value)
    {
        Debug.Log($"<color=purple><b>[EVENT RAISED]</b></color> String Event <b>'{this.name}'</b> was raised with value: <b>'{value}'</b>.", this);
        OnEventRaised?.Invoke(value);
    }
}