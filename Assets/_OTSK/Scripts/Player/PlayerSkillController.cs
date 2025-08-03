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
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerHealthManaNoise == null) playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();

        InitializeSkills();
    }
    private void Start()
    {
        // Subscribe to the CombatManager's events using named methods.
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart += HandleCombatStart;
            CombatManager.Instance.OnCombatEnd += HandleCombatEnd;
        }
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

        // --- Special, SAFE Check for Scrying Toggle ---
        if (skill.skillID == SkillIdentifier.Scrying)
        {
            // First, check if the system is ready before trying to use it.
            if (ScryingSystem.Instance != null)
            {
                // If the system is ready, THEN we check if the skill is active.
                if (ScryingSystem.Instance.IsScryingActive)
                {
                    Debug.Log("Scrying is already active. Executing toggle-off.");
                    SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
                    return; // Exit here after toggling off.
                }
            }
            else
            {
                // If this log appears, it confirms the ScryingSystem isn't ready in time.
                Debug.LogError("SKILL CHECK FAILED: ScryingSystem.Instance was not ready!");
                return; // Block the skill use if the system isn't ready.
            }
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
}