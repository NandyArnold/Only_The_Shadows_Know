// ObjectiveProgressData.cs
// This is just a data container, so it doesn't inherit from anything.
[System.Serializable]
public struct ObjectiveProgressData
{
    public string counterLabel; // e.g., "Scouts Killed"
    public int currentProgress; // e.g., 1
    public int requiredAmount;  // e.g., 3
}