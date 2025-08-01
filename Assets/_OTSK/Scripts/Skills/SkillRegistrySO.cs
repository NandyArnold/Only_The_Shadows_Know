// SkillRegistrySO.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "SkillRegistry", menuName = "Only The Shadows Know/Skills/Skill Registry")]
public class SkillRegistrySO : ScriptableObject
{
    [SerializeField]
    private List<SkillSO> allSkills;

    private Dictionary<SkillIdentifier, SkillSO> _skillDictionary;

    public void Initialize()
    {
        _skillDictionary = allSkills.ToDictionary(skill => skill.skillID, skill => skill);
        Debug.Log("Skill Registry Initialized.");
    }

    public SkillSO GetSkill(SkillIdentifier skillID)
    {
        _skillDictionary.TryGetValue(skillID, out SkillSO skill);
        return skill;
    }
}