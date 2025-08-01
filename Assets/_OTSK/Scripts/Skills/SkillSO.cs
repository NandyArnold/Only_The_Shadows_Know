// SkillSO.cs - UPDATED

using UnityEngine;

[CreateAssetMenu(fileName = "Skill_New", menuName = "Only The Shadows Know/Skills/Skill Data")]
public class SkillSO : ScriptableObject
{
    [Header("Skill Info")]
    public SkillIdentifier skillID;
    public SkillType skillType;

    public string skillName;
    public Sprite icon;
    [TextArea]
    public string description;

    [Header("Skill Stats")]
    public float manaCost;
    public float cooldown;

    // REPLACED the old boolean with our new, more flexible enum.
    public SkillUsageCondition usageCondition;

    [Header("Skill Behavior")]
    [Tooltip("The script that defines what this skill actually does.")]
    public SkillEffectSO skillEffectData;
}