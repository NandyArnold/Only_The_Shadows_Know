// PlayerAnimationController.cs - FINAL REFACTORED VERSION

using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // --- CORE LOCOMOTION HASHES (Kept) ---
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveYHash = Animator.StringToHash("MoveY");
    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int jumpTriggerHash = Animator.StringToHash("JumpTrigger");
    private readonly int dodgeRollTriggerHash = Animator.StringToHash("DodgeRollTrigger");
    private readonly int standardJumpTriggerHash = Animator.StringToHash("StandardJumpTrigger");
    private readonly int runningJumpTriggerHash = Animator.StringToHash("RunningJumpTrigger");
    private readonly int isAimingHash = Animator.StringToHash("IsAiming");



    // --- WEAPON ANIMATION ---
    private IWeaponAnimation _currentWeaponAnimation;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // PlayerCombat will call this when a weapon is equipped.
    public void SetWeaponAnimation(IWeaponAnimation weaponAnimation)
    {
        _currentWeaponAnimation = weaponAnimation;
    }

    public void SetAimingState(bool isAiming)
    {
        if (animator == null) return;
        animator.SetBool(isAimingHash, isAiming);
    }


    // --- DELEGATION TO WEAPON ---
    // PlayerCombat calls this, and we pass the command to the current weapon animator.
    public void TriggerPrimaryAttack()
    {
        _currentWeaponAnimation?.PlayPrimaryAttack();
        Debug.Log("2. PlayerAnimationController received TriggerPrimaryAttack command.");
        Debug.Log("--> Current Weapon Animation is: " + _currentWeaponAnimation?.GetType().Name);

    }

    public void TriggerSecondaryAttack()
    {
        _currentWeaponAnimation?.PlaySecondaryAttack();
    }

    // --- CORE LOCOMOTION METHODS (Kept) ---

    public void SetLocomotionInput(float moveX, float moveY, bool isCrouching, bool isRunning)
    {
        if (animator == null) return;
        animator.SetFloat(moveXHash, moveX);
        animator.SetFloat(moveYHash, moveY);
        animator.SetBool(isCrouchingHash, isCrouching);
        animator.SetBool(isRunningHash, isRunning);
    }

    public void PlayStandardJumpAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(standardJumpTriggerHash);
    }

    public void PlayRunningJumpAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(runningJumpTriggerHash);
    }

    public void PlayDodgeRollAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(dodgeRollTriggerHash);
    }

    //public void SetAimingState(bool isAiming)
    //{
    //    _currentWeaponAnimation?.SetAiming(isAiming);
    //}
}