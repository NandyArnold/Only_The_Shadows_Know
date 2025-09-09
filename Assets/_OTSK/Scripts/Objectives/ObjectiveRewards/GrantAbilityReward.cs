// GrantAbilityReward.cs

using UnityEngine;

[System.Serializable]
public class GrantAbilityReward : RewardAction
{
    [Tooltip("The ability to grant. Can be an UnlockableAbilitySO, WeaponSO, or SkillSO.")]
    [SerializeField] private ScriptableObject abilityToGrant;

    public override void ExecuteReward()
    {
        if (abilityToGrant != null && AbilityUnlockManager.Instance != null)
        {
            AbilityUnlockManager.Instance.GrantAbility(abilityToGrant);
            //Debug.Log($"<color=lime>REWARD GRANTED:</color> Unlocked {abilityToGrant.name}");
        }
    }
}