using UnityEngine;

[System.Serializable]
public class ResistanceEntry
{
    public DamageTypeSO damageType;
    [Tooltip("Damage is multiplied by this value. 0 = immune, 0.5 = resistant, 2.0 = vulnerable. Use a huge number for one-shot kills.")]
    public float multiplier = 1f;
}