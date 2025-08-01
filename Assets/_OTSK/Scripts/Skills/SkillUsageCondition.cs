// SkillUsageCondition.cs

// Defines the conditions under which a skill can be used.
public enum SkillUsageCondition
{
    Always,          // Can be used anytime.
    CombatOnly,      // Can only be used when in combat.
    OutOfCombatOnly  // Can only be used when not in combat.
}