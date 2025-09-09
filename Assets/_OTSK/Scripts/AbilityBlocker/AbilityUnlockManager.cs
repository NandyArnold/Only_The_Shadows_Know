// Create this new script, AbilityUnlockManager.cs
using System.Collections.Generic;
using UnityEngine;

public class AbilityUnlockManager : MonoBehaviour // Add your ISaveable implementation here
{
    public static AbilityUnlockManager Instance { get; private set; }

    // We use ScriptableObject as the key, which can be a WeaponSO, SkillSO, or UnlockableAbilitySO.
    private readonly HashSet<ScriptableObject> _unlockedAbilities = new HashSet<ScriptableObject>();
    private readonly HashSet<ScriptableObject> _temporarilyLockedAbilities = new HashSet<ScriptableObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GrantAbility(ScriptableObject ability)
    {
        if (ability == null) return;
        if (!_unlockedAbilities.Contains(ability))
        {
            _unlockedAbilities.Add(ability);
            //Debug.Log($"<color=cyan>ABILITY UNLOCKED:</color> {ability.name}");
        }
    }

    public void RevokeAbility(ScriptableObject ability)
    {
        if (ability != null && _unlockedAbilities.Contains(ability))
        {
            _unlockedAbilities.Remove(ability);
            Debug.Log($"<color=orange>ABILITY REVOKED:</color> {ability.name}");
        }
    }

    public void TemporarilyLock(ScriptableObject ability)
    {
        if (ability != null && !_temporarilyLockedAbilities.Contains(ability))
        {
            _temporarilyLockedAbilities.Add(ability);
        }
    }

    public void ReEnableAbility(ScriptableObject ability)
    {
        if (ability != null && _temporarilyLockedAbilities.Contains(ability))
        {
            _temporarilyLockedAbilities.Remove(ability);
        }
    }

    public bool IsAbilityAvailable(ScriptableObject ability)
    {
        if (ability == null) return false; // If there's no ability data, it's not available.
        return _unlockedAbilities.Contains(ability) && !_temporarilyLockedAbilities.Contains(ability);
    }

    // --- ISaveable Implementation (Example) ---
    // public object CaptureState() => new List<string>(/* logic to save SO names or GUIDs */);
    // public void RestoreState(object state) { /* logic to load SOs from Resources or Addressables */ }
}