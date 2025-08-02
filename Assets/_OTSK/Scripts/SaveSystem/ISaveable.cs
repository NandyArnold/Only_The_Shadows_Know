public interface ISaveable
{
    // A unique identifier for this object.
    string UniqueID { get; }

    // Gathers the object's current state into a serializable object.
    object CaptureState();

    // Restores the object's state from the given data.
    void RestoreState(object state);
}