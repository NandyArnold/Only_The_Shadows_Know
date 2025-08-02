// PlayerStateData.cs - ESave Compatible Version
using Esper.ESave; // Add this namespace
using Esper.ESave.SavableObjects; // Add this namespace for SavableVector

[System.Serializable]
public class PlayerStateData
{
    // Use ESave's savable vector types instead of Unity's
    public SavableVector position;
    public SavableVector rotation; // Quaternions are stored as a savable Vector4 (x,y,z,w)

    public float currentHealth;
    public float currentMana;
}