// In BowAnimation.cs
using UnityEngine;

public class BowAnimation : MonoBehaviour, IWeaponAnimation
{
    [SerializeField] private Animator animator;

    private readonly int isAimingHash = Animator.StringToHash("IsAiming");
    private readonly int fireTriggerHash = Animator.StringToHash("Bow_Fire"); // NEW

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    //public void SetAiming(bool isAiming)
    //{
    //    animator.SetBool(isAimingHash, isAiming);
    //}

    // This method is now filled in.
    public void PlayPrimaryAttack()
    {
        Debug.Log("3. BowAnimation is playing the Bow_Fire trigger.");
        animator.SetTrigger(fireTriggerHash);
    }

    public void PlaySecondaryAttack() { /* The bow doesn't use a separate secondary attack animation */ }
}