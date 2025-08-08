// PlayerAnimationController.cs - FINAL REFACTORED VERSION

using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // --- CORE LOCOMOTION HASHES (Kept) ---
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveYHash = Animator.StringToHash("MoveY");
    private readonly int speedHash = Animator.StringToHash("Speed");

    private readonly int isCrouchingHash = Animator.StringToHash("IsCrouching");

    private readonly int isRunningHash = Animator.StringToHash("IsRunning");

    private readonly int dodgeRollTriggerHash = Animator.StringToHash("DodgeRollTrigger");

    private readonly int standardJumpTriggerHash = Animator.StringToHash("StandardJumpTrigger");
    private readonly int runningJumpTriggerHash = Animator.StringToHash("RunningJumpTrigger");

    private readonly int isAimingHash = Animator.StringToHash("IsAiming");
    private readonly int weaponTypeHash = Animator.StringToHash("WeaponType");

    private readonly int spectralStateHash = Animator.StringToHash("SpectralState");

    private readonly int riftPlaceTriggerHash = Animator.StringToHash("Rift_Place");
    private readonly int riftTeleportTriggerHash = Animator.StringToHash("Rift_Teleport");

    private readonly int scryingCastTriggerHash = Animator.StringToHash("Scrying_Cast");




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
        //Debug.Log("2. PlayerAnimationController received TriggerPrimaryAttack command.");
        //Debug.Log("--> Current Weapon Animation is: " + _currentWeaponAnimation?.GetType().Name);

    }

    public void TriggerSecondaryAttack()
    {
        _currentWeaponAnimation?.PlaySecondaryAttack();
    }
    public void TriggerTertiaryAttack()
    {
        _currentWeaponAnimation?.PlayTertiaryAttack();
    }

    public void TriggerDagger_Finisher()
    {
        if (_currentWeaponAnimation is DaggerAnimation daggerAnim)
        {
            daggerAnim.PlayFinisherAnimation();
        }
        else
        {
            Debug.LogWarning("Current weapon is not a DaggerAnimation, cannot play finisher.");
        }
    }

    // --- CORE LOCOMOTION METHODS (Kept) ---

    public void SetLocomotionInput(float moveX, float moveY, bool isCrouching, bool isRunning)
    {
        if (animator == null) return;

        animator.SetFloat(moveXHash, moveX);
        animator.SetFloat(moveYHash, moveY);

        float speed = new Vector2(moveX, moveY).magnitude;
        animator.SetFloat(speedHash, speed);

        animator.SetBool(isCrouchingHash, isCrouching);
        animator.SetBool(isRunningHash, isRunning);
    }

    public void PlayAnimationTrigger(string triggerName)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(Animator.StringToHash(triggerName));
        }
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

    public void SetWeaponType(int type)
    {
        animator.SetInteger(weaponTypeHash, type);
    }

    public void SetSpectralState(int state)
    {
        animator.SetInteger(spectralStateHash, state);
    }

    public void PlayRiftPlaceAnimation()
    {
        animator.SetTrigger(riftPlaceTriggerHash);
    }

    public void PlayRiftTeleportAnimation()
    {
        animator.SetTrigger(riftTeleportTriggerHash);
    }
    public void PlayScryingCastAnimation()
    {
        animator.SetTrigger(scryingCastTriggerHash);
    }
}