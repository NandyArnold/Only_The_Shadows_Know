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
    public float manaCostOverTime;
    public SkillUsageCondition usageCondition;
    public CastMode castMode;

    [Header("UI Info")]
    public string keybindText;



    [Tooltip("For 'Targeted' cast skills, should this lock the player's movement during the cast animation?")]
    public bool lockMovementWhileCasting; // This is now clearly for Targeted skills only.

    [Header("Skill Behavior")]
    [Tooltip("The script that defines what this skill actually does.")]
    public SkillEffectSO skillEffectData;
}