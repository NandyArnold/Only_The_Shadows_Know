// PlayerSkillController.cs - CORRECTED and STABLE VERSION

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
    private bool _isInCombat = false;

    private void Awake()
    {
        
    }
    private void Start()
    {
        // Subscribe to the CombatManager's events using named methods.
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart += HandleCombatStart;
            CombatManager.Instance.OnCombatEnd += HandleCombatEnd;
        }
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerHealthManaNoise == null) playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();

        InitializeSkills();
    }
    private void InitializeSkills()
    {
        if (skillRegistry == null)
        {
            Debug.LogError("Skill Registry is not assigned!", this);
            return;
        }
        
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

        // Unsubscribe from the named methods. This is stable.
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart -= HandleCombatStart;
            CombatManager.Instance.OnCombatEnd -= HandleCombatEnd;
        }
    }

    // --- NEW Event Handlers ---
    private void HandleCombatStart() => _isInCombat = true;
    private void HandleCombatEnd() => _isInCombat = false;


    private void TryActivateSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= _equippedSkills.Count) return;

        SkillSO skill = _equippedSkills[skillIndex];
        if (skill == null)
        {
            Debug.LogError($"SKILL FAILED: No skill data found at index {skillIndex} in the Skill Registry.");
            return;
        }

        // --- Special Toggle-Off Logic Block ---
        if (skill.skillID == SkillIdentifier.Scrying && ScryingSystem.Instance != null && ScryingSystem.Instance.IsScryingActive)
        {
            Debug.Log("Scrying is already active. Executing toggle-off.");
            SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
            return;
        }
        // ADD THIS to handle the Endwalker toggle
        else if (skill.skillID == SkillIdentifier.Endwalker && GetComponent<PlayerController>().IsInEndwalkerState)
        {
            Debug.Log("Endwalker is already active. Executing toggle-off.");
            SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
            return;
        }


        // --- Standard Validation Checks ---
        if (SkillCooldownManager.Instance.IsOnCooldown(skill.skillID))
        {
            Debug.Log($"<color=red>SKILL FAILED:</color> '{skill.skillName}' is on cooldown.");
            return;
        }

        if (playerHealthManaNoise.CurrentMana < skill.manaCost)
        {
            Debug.Log($"<color=red>SKILL FAILED:</color> Not enough mana for '{skill.skillName}'.");
            return;
        }

        switch (skill.usageCondition)
        {
            case SkillUsageCondition.CombatOnly:
                if (!_isInCombat)
                {
                    Debug.Log($"<color=red>SKILL FAILED:</color> Cannot use '{skill.skillName}' outside of combat.");
                    return;
                }
                break;
            case SkillUsageCondition.OutOfCombatOnly:
                if (_isInCombat)
                {
                    Debug.Log($"<color=red>SKILL FAILED:</color> Cannot use '{skill.skillName}' while in combat.");
                    return;
                }
                break;
        }

        Debug.Log($"<color=green>SKILL ACTIVATED:</color> {skill.skillName}");
        playerHealthManaNoise.ConsumeMana(skill.manaCost);
        SkillCooldownManager.Instance.StartCooldown(skill.skillID, skill.cooldown);
        SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
    }

    public void TryActivateSkill(SkillIdentifier skillID)
    {
        // Find the index of the skill in our equipped list.
        int skillIndex = _equippedSkills.FindIndex(s => s != null && s.skillID == skillID);

        if (skillIndex != -1)
        {
            // If we found it, call our original private method with the correct index.
            TryActivateSkill(skillIndex);
        }
        else
        {
            Debug.LogWarning($"Attempted to activate skill '{skillID}' but it is not equipped or found.");
        }
    }
}