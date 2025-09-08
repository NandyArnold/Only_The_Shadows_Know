// ObjectiveProgressEvent.cs
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ObjectiveProgressEvent", menuName = "Only The Shadows Know/Events/Objective Progress Event")]
public class ObjectiveProgressEvent : ScriptableObject
{
    public UnityAction<ObjectiveProgressData> OnEventRaised;

    public void Raise(ObjectiveProgressData data)
    {
        OnEventRaised?.Invoke(data);
    }
}