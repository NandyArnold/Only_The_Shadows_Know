// ObjectiveGoal.cs

using System;


[Serializable]
public abstract class ObjectiveGoal
{
    // A reference to the runtime objective instance that owns this goal.
    // The 'protected' keyword means only this class and its children can access it.
    protected ObjectiveInstance owner;

    public int currentAmount;
    public int requiredAmount = 1;

    // A flag to check if we've successfully subscribed to game events.
    private bool isSubscribed = false;

    /// <summary>
    /// Initializes the goal, giving it a reference to its owner.
    /// This is where we will subscribe to necessary game events.
    /// </summary>
    public virtual void Initialize(ObjectiveInstance owner)
    {
        this.owner = owner;
        if (!isSubscribed)
        {
            SubscribeToEvents();
            isSubscribed = true;
        }
    }

    /// <summary>
    /// Cleans up the goal, unsubscribing from events to prevent memory leaks.
    /// </summary>
    public virtual void CleanUp()
    {
        if (isSubscribed)
        {
            UnsubscribeFromEvents();
            isSubscribed = false;
        }
    }

    /// <summary>
    /// This is where child classes will subscribe to specific game events
    /// (e.g., OnEnemyDied, OnItemPickedUp).
    /// </summary>
    protected abstract void SubscribeToEvents();

    /// <summary>
    /// This is where child classes will unsubscribe from the events they subscribed to.
    /// </summary>
    protected abstract void UnsubscribeFromEvents();

    /// <summary>
    /// Checks if the goal is complete.
    /// </summary>
    public bool IsComplete() => (currentAmount >= requiredAmount);

    /// <summary>
    /// Call this from child classes when progress is made.
    /// </summary>
    protected void Evaluate()
    {
        if (IsComplete())
        {
            // Tell the owner that this goal is complete.
            owner.OnGoalCompleted(this);
        }
    }
}