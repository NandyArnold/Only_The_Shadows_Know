using System;

public interface IObjectiveTrigger
{
    string TriggerID { get; }
    event Action<string> OnTriggerActivated;
}
