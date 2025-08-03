// SkillExecutor.cs
using UnityEngine;
using System.Collections;

public class SkillExecutor : MonoBehaviour
{
    public static SkillExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void ExecuteSkill(SkillSO skill, GameObject caster)
    {
        if (skill.skillEffectData == null)
        {
            Debug.LogWarning($"Skill '{skill.skillName}' has no SkillEffectData assigned!");
            return;
        }

        // This is the payoff for our polymorphic design. We just tell the effect to activate.
        skill.skillEffectData.Activate(caster);
    }

    public void ExecuteCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}