// SkillType.cs

// Describes the category or 'type' of a skill for classification purposes.
// SkillType.cs - UPGRADED

using System;

[Flags] // This is the magic attribute that enables multi-selection.
public enum SkillType
{
    Utility = 1 << 0, // 1
    Movement = 1 << 1, // 2
    RangedDamage = 1 << 2, // 4
    MeleeDamage = 1 << 3, // 8
    Stealth = 1 << 4, // 16
    Debuff = 1 << 5, // 32
    DamageOverTime = 1 << 6,  // 64
    Buff = 1 << 7, // 128

}
