// ObjectiveGoal.cs

using System;
using UnityEngine;


[Serializable]
public abstract class ObjectiveGoal
{
    // A reference to the runtime objective instance that owns this goal.
    // The 'protected' keyword means only this class and its children can access it.
    protected ObjectiveInstance owner;

    public int currentAmount;
    public int requiredAmount = 1;

    [Header("Progress UI Settings")]
    [Tooltip("The text to display next to the counter (e.g., 'Enemies Killed').")]
    [SerializeField]  public string counterLabel;
    [Tooltip("The event to raise when progress is made.")]
    [SerializeField]  public ObjectiveProgressEvent onProgressUpdated;

    // A flag to check if we've successfully subscribed to game events.
    private bool isSubscribed = false;

    protected ObjectiveGoal(ObjectiveGoal dataFromSO)
    {
        this.requiredAmount = dataFromSO.requiredAmount;
        this.counterLabel = dataFromSO.counterLabel;
        this.onProgressUpdated = dataFromSO.onProgressUpdated;
    }

    protected ObjectiveGoal() { }

    public virtual void Initialize(ObjectiveInstance owner)
    {
        this.owner = owner;
        if (!isSubscribed)
        {
            SubscribeToEvents();
            isSubscribed = true;
        }
    }

    public virtual void CleanUp()
    {
        if (isSubscribed)
        {
            UnsubscribeFromEvents();
            isSubscribed = false;
        }
    }


    public void UpdateUI()
    {
        Debug.Log($"<color=yellow>[ObjectiveGoal]</color> UpdateUI called. Event channel is null:" +
            $" {onProgressUpdated == null}. Label is: '{counterLabel}'");

        if (onProgressUpdated != null)
        {
            var data = new ObjectiveProgressData
            {
                counterLabel = this.counterLabel,
                currentProgress = currentAmount,
                requiredAmount = requiredAmount
            };
            Debug.Log($"<color=yellow>[ObjectiveGoal]</color> Packaged data: Label='{data.counterLabel}'," +
                $" Progress={data.currentProgress}/{data.requiredAmount}. Raising event...");
            onProgressUpdated.Raise(data);
        }
    }

    protected abstract void SubscribeToEvents();

    protected abstract void UnsubscribeFromEvents();

    public bool IsComplete() => (currentAmount >= requiredAmount);

    protected void Evaluate()
    {
        if (IsComplete())
        {
            // Tell the owner that this goal is complete.
            owner.OnGoalCompleted(this);
        }
    }
}