// ObjectiveProgressEvent.cs
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ObjectiveProgressEvent", menuName = "Only The Shadows Know/Events/Objective Progress Event")]
public class ObjectiveProgressEvent : ScriptableObject
{
    public UnityAction<ObjectiveProgressData> OnEventRaised;

    public void Raise(ObjectiveProgressData data)
    {
        Debug.Log($"<color=purple>[EVENT RAISED]</color> ObjectiveProgressEvent raised with label: " +
            $"'{data.counterLabel}' and progress {data.currentProgress}/{data.requiredAmount}");
        OnEventRaised?.Invoke(data);
    }
}