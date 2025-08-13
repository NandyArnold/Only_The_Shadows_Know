// EnemyAnimationController.cs - COMPLETE VERSION

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;

    // Parameter Hashes
    private readonly int deathTriggerHash = Animator.StringToHash("DeathTrigger");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int combatStateHash = Animator.StringToHash("CombatState");
    private readonly int primaryAttackTriggerHash = Animator.StringToHash("PrimaryAttack");
    private readonly int lookAroundTriggerHash = Animator.StringToHash("LookAroundTrigger");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Sets the "Speed" float parameter in the Animator to control locomotion blend trees.
    /// </summary>
    public void SetSpeed(float speed)
    {
        _animator.SetFloat(speedHash, speed);
    }

    /// <summary>
    /// Sets the "CombatState" float parameter in the Animator (0 for normal, 1 for combat).
    /// </summary>
    public void SetIsInCombat(bool inCombat)
    {
        _animator.SetFloat(combatStateHash, inCombat ? 1f : 0f);
    }

    public void PlayDeathAnimation()
    {
        _animator.SetTrigger(deathTriggerHash);
    }

    public void PlayPrimaryAttackAnimation()
    {
        _animator.SetTrigger(primaryAttackTriggerHash);
    }

    public void PlayLookAroundAnimation()
    {
        _animator.SetTrigger(lookAroundTriggerHash);
    }
}