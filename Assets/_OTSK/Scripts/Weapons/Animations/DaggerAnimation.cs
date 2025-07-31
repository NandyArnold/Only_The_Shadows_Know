// DaggerAnimation.cs

using UnityEngine;

public class DaggerAnimation : MonoBehaviour, IWeaponAnimation
{
    [SerializeField] private Animator animator;

    // Hashes for the dagger's specific animation triggers
    private readonly int daggerPrimaryAttackTrigger = Animator.StringToHash("Dagger_PrimaryAttack");
    private readonly int daggerSecondaryAttackTrigger = Animator.StringToHash("Dagger_SecondaryAttack");
    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayPrimaryAttack()
    {
        animator.SetTrigger(daggerPrimaryAttackTrigger);
    }

    public void PlaySecondaryAttack()
    {
        animator.SetTrigger(daggerSecondaryAttackTrigger);
    }
}