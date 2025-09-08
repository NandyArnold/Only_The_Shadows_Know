// RunEventReward.cs

using UnityEngine;

[System.Serializable]
public class RunEventReward : RewardAction
{
    [Tooltip("The GameEvent asset to raise when this reward is granted.")]
    [SerializeField] private GameEvent eventToRaise;

    public override void ExecuteReward()
    {
        if (eventToRaise != null)
        {
            eventToRaise.Raise();
            Debug.Log($"<color=lime>REWARD GRANTED:</color> Raised Game Event {eventToRaise.name}");
        }
    }
}