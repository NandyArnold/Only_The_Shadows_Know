// SkillRegistrySO.cs - SELF-INITIALIZING VERSION
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "SkillRegistry", menuName = "Only The Shadows Know/Skills/Skill Registry")]
public class SkillRegistrySO : ScriptableObject
{
    [SerializeField]
    private List<SkillSO> allSkills;

    private Dictionary<SkillIdentifier, SkillSO> _skillDictionary;
    private bool _isInitialized = false;

    private void OnEnable()
    {
        _isInitialized = false; // Reset on enable
    }

    public void Initialize()
    {
        if (_isInitialized) return;
        _skillDictionary = allSkills.ToDictionary(skill => skill.skillID, skill => skill);
        _isInitialized = true;
        Debug.Log("Skill Registry Initialized.");
    }

    public SkillSO GetSkill(SkillIdentifier skillID)
    {
        // "Lazy Initialization": If we try to get a skill before the dictionary is ready,
        // we initialize it right here. This makes the system bulletproof.
        if (!_isInitialized)
        {
            Initialize();
        }

        _skillDictionary.TryGetValue(skillID, out SkillSO skill);
        return skill;
    }
}