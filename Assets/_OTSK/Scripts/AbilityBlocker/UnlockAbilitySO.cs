// This is only for stuff that has like no scriptable objects, no weapons, no skills, just general abilities
using UnityEngine;

[CreateAssetMenu(fileName = "Ability_", menuName = "Abilities/Unlockable Ability Definition")]
public class UnlockableAbilitySO : ScriptableObject
{
    [Tooltip("The unique identifier for this ability. Used for programmatic checks if needed.")]
    public AbilityIdentifier abilityID;

    [Tooltip("Display name for UI purposes.")]
    public string displayName;

    [Tooltip("Icon to show in skill trees or tutorials.")]
    public Sprite icon;
}

// Create this new enum, AbilityIdentifier.cs (or modify your existing one)
// This should contain ONLY general-purpose abilities, not specific weapons or skills.
public enum AbilityIdentifier
{
    None,
    // Movement
    Run,
    Crouch,
    Jump,
    Dodge,
    // Add any other core mechanics here
}