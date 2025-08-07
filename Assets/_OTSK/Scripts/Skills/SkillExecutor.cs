// SkillExecutor.cs
using UnityEngine;
using System.Collections;
using System;

public class SkillExecutor : MonoBehaviour
{
    public static SkillExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    public static event Action<SkillSO> OnSkillConfirmed;
    public void ExecuteSkill(SkillSO skill, GameObject caster)
    {
        if (skill.skillEffectData == null)
        {
            Debug.LogWarning($"Skill '{skill.skillName}' has no SkillEffectData assigned!");
            return;
        }
        // This is the payoff for our polymorphic design. We just tell the effect to activate.
        skill.skillEffectData.Activate(caster);
        OnSkillConfirmed?.Invoke(skill);
    }

    public void ExecuteCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}