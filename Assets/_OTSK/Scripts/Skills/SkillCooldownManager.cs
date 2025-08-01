// SkillCooldownManager.cs
using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    public static SkillCooldownManager Instance { get; private set; }

    // Stores the time when a skill's cooldown will end.
    private readonly Dictionary<SkillIdentifier, float> _cooldowns = new Dictionary<SkillIdentifier, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        // This doesn't need to persist as it's part of _GameSystems already.
    }

    public void StartCooldown(SkillIdentifier skillID, float duration)
    {
        _cooldowns[skillID] = Time.time + duration;
    }

    public bool IsOnCooldown(SkillIdentifier skillID)
    {
        // If the skill is in the dictionary and its end time is in the future, it's on cooldown.
        return _cooldowns.TryGetValue(skillID, out float endTime) && Time.time < endTime;
    }

    public float GetRemainingCooldown(SkillIdentifier skillID)
    {
        if (_cooldowns.TryGetValue(skillID, out float endTime))
        {
            return Mathf.Max(0, endTime - Time.time);
        }
        return 0;
    }
}