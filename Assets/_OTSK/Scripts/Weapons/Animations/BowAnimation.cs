// Create this new script: BowAnimation.cs
using UnityEngine;

public class BowAnimation : MonoBehaviour, IWeaponAnimation
{
    [SerializeField] private Animator animator;
    private readonly int isAimingHash = Animator.StringToHash("IsAiming");

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void SetAiming(bool isAiming)
    {
        animator.SetBool(isAimingHash, isAiming);
    }

    public void PlayPrimaryAttack() { /* For unfocused shot */ }
    public void PlaySecondaryAttack() { /* Not used by bow */ }
}