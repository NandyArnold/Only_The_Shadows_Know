// PlayerSkillController.cs

using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerHealthManaNoise playerHealthManaNoise;

    [Header("Skill Data")]
    [SerializeField] private SkillRegistrySO skillRegistry;
    [SerializeField] private List<SkillIdentifier> equippedSkillIDs;

    private List<SkillSO> _equippedSkills;

    private void Awake()
    {
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerHealthManaNoise == null) playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();

        InitializeSkills();
    }

    private void InitializeSkills()
    {
        skillRegistry.Initialize();
        _equippedSkills = new List<SkillSO>();
        foreach (var skillID in equippedSkillIDs)
        {
            _equippedSkills.Add(skillRegistry.GetSkill(skillID));
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnSkillInput += TryActivateSkill;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnSkillInput -= TryActivateSkill;
        }
    }

    private void TryActivateSkill(int skillIndex)
    {
        // 1. Validate the index
        if (skillIndex < 0 || skillIndex >= _equippedSkills.Count) return;

        SkillSO skill = _equippedSkills[skillIndex];
        if (skill == null) return;

        // 2. Check cooldown
        if (SkillCooldownManager.Instance.IsOnCooldown(skill.skillID))
        {
            Debug.Log($"Skill '{skill.skillName}' is on cooldown.");
            return;
        }

        // 3. Check mana cost
        if (playerHealthManaNoise.CurrentMana < skill.manaCost)
        {
            Debug.Log($"Not enough mana for '{skill.skillName}'.");
            return;
        }

        // 4. TODO: Check Usage Condition (e.g., CombatOnly vs OutOfCombatOnly)
        // We will add this check later when we have a combat state.

        // --- All checks passed, execute the skill ---
        playerHealthManaNoise.ConsumeMana(skill.manaCost);
        SkillCooldownManager.Instance.StartCooldown(skill.skillID, skill.cooldown);
        SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);

        // TODO: Play a casting animation
    }
}