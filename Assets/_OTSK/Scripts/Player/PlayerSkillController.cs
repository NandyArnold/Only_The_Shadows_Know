using System.Collections;
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
    private SkillSO _channeledSkill;
    private Coroutine _manaDrainCoroutine;
    private Coroutine _channelingCoroutine;
    private bool _isActivatingASkill = false;

    private void Awake()
    {
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerHealthManaNoise == null) playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();
    }

    private void Start()
    {
        InitializeSkills();
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
            playerInputHandler.OnSkillPerformedInput += TryActivateSkill;
            playerInputHandler.OnSkillCanceledInput += HandleSkillCanceled;

            playerInputHandler.OnCancelActionInput += HandleCancelAction;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnSkillPerformedInput -= TryActivateSkill;
            playerInputHandler.OnSkillCanceledInput -= HandleSkillCanceled;

            playerInputHandler.OnCancelActionInput -= HandleCancelAction;
        }
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStart -= HandleCombatStart;
            CombatManager.Instance.OnCombatEnd -= HandleCombatEnd;
        }
    }

    private void HandleCombatStart() => _isInCombat = true;
    private void HandleCombatEnd() => _isInCombat = false;

    public void TryActivateSkill(SkillIdentifier skillID)
    {
        int skillIndex = _equippedSkills.FindIndex(s => s != null && s.skillID == skillID);
        if (skillIndex != -1) TryActivateSkill(skillIndex);
    }

    private void TryActivateSkill(int skillIndex)
    {
        if (_isActivatingASkill || _channeledSkill != null) return;
        if (skillIndex < 0 || skillIndex >= _equippedSkills.Count) return;

        SkillSO skill = _equippedSkills[skillIndex];
        if (skill == null) return;

        if (skill.skillID == SkillIdentifier.Scrying && ScryingSystem.Instance != null && ScryingSystem.Instance.IsScryingActive)
        {
            SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
            return;
        }
        else if (skill.skillID == SkillIdentifier.Endwalker && GetComponent<PlayerController>().IsInEndwalkerState)
        {
            SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
            return;
        }

        if (SkillCooldownManager.Instance.IsOnCooldown(skill.skillID)) return;
        if (playerHealthManaNoise.CurrentMana < skill.manaCost) return;

        switch (skill.usageCondition)
        {
            case SkillUsageCondition.CombatOnly: if (!_isInCombat) return; break;
            case SkillUsageCondition.OutOfCombatOnly: if (_isInCombat) return; break;
        }

        // --- EXECUTION LOGIC IS NOW ONLY HERE ---
        _isActivatingASkill = true;
        playerHealthManaNoise.ConsumeMana(skill.manaCost);
        Debug.Log($"<color=green>SKILL ACTIVATED:</color> {skill.skillName}");

        switch (skill.castMode)
        {
            case CastMode.Instant:
            case CastMode.Targeted:
                SkillCooldownManager.Instance.StartCooldown(skill.skillID, skill.cooldown);
                SkillExecutor.Instance.ExecuteSkill(skill, this.gameObject);
                StartCoroutine(SkillActivationGracePeriod());
                break;
            case CastMode.Channel:
                _channeledSkill = skill;
                _channelingCoroutine = StartCoroutine(skill.skillEffectData.StartChannel(this.gameObject));
                _manaDrainCoroutine = StartCoroutine(DrainManaOverTime(skill));
                break;
        }
    }

    private void HandleSkillCanceled(int skillIndex)
    {
        if (_channeledSkill != null && skillIndex < _equippedSkills.Count && _equippedSkills[skillIndex] == _channeledSkill)
        {
            _channeledSkill.skillEffectData.StopChannel(this.gameObject);
            if (_manaDrainCoroutine != null) StopCoroutine(_manaDrainCoroutine);
            if (_channelingCoroutine != null) StopCoroutine(_channelingCoroutine);
            _channeledSkill = null;
            _isActivatingASkill = false; // Release the lock
        }
    }

    private void HandleCancelAction()
    {
        // Check the static property on the effect script and call the static cancel method.
        if (TariaksuqsRiftEffectSO.IsRiftActive)
        {
            TariaksuqsRiftEffectSO.CancelRift();
        }
    }

    private IEnumerator SkillActivationGracePeriod()
    {
        yield return new WaitForSeconds(0.1f);
        _isActivatingASkill = false;
    }

    private IEnumerator DrainManaOverTime(SkillSO skill)
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!playerHealthManaNoise.ConsumeMana(skill.manaCostOverTime))
            {
                HandleSkillCanceled(System.Array.IndexOf(_equippedSkills.ToArray(), skill));
                yield break;
            }
        }
    }
}