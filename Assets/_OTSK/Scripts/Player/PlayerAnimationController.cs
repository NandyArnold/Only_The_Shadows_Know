using UnityEngine;

// Enums for clarity and consistency across scripts (PlayerMovement, PlayerCombat, PlayerSkillController)
public enum CombatState { Idle, PrimaryAttack, SecondaryAttack, Aiming }
public enum SkillState { None, SpectralShift, Scrying, DeathZone, Endwalker }

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator; // Assign this in the Inspector or it will try to find it on children

    // Animator Parameter Hashes for efficiency (good practice)
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming"); // For later, with Bow/Animancy
    private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");
    private static readonly int DodgeRollTriggerHash = Animator.StringToHash("DodgeRollTrigger");
    private static readonly int PrimaryAttackTriggerHash = Animator.StringToHash("PrimaryAttackTrigger"); // For later
    private static readonly int SkillCastTriggerHash = Animator.StringToHash("SkillCastTrigger"); // For later

    private void Awake()
    {
        // Try to find the Animator component on this GameObject or its children if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("PlayerAnimationController: Animator not found on this GameObject or its children! Please assign it in the Inspector or ensure it's present.");
            }
        }
    }

    // --- Locomotion Input (Called by PlayerMovement) ---
    public void SetLocomotionInput(float moveX, float moveY, bool isCrouching, bool isRunning)
    {
        if (animator == null) return;

        // Set float parameters for 2D blend trees
        animator.SetFloat(MoveXHash, moveX);
        animator.SetFloat(MoveYHash, moveY);

        // Set boolean parameter for crouching state
        animator.SetBool(IsCrouchingHash, isCrouching);

        // NOTE: We no longer directly control Speed like a 1D blend tree here.
        // The 2D blend tree's thresholds and the transitions between Locomotion,
        // CrouchLocomotion, and AimedLocomotion (based on IsCrouching/IsAiming)
        // will now handle the blending of walk/run/crouch animations.
        // The 'isRunning' boolean might be used by a transition or nested blend tree if needed.
        // For example, you might have two blend trees for Locomotion: one for walk, one for run,
        // and transition between them using an 'IsRunning' bool, or have different
        // X/Y thresholds in a single 2D blend tree for walk vs. run.
        // Our current setup assumes the blend tree positions (0,1) vs (0,2) etc. handle speed.
    }

    // --- Action Triggers (Called by PlayerMovement/PlayerCombat/PlayerSkillController) ---

    public void PlayJumpAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(JumpTriggerHash);
    }

    public void PlayDodgeRollAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(DodgeRollTriggerHash);
    }

    // --- Combat States (For PlayerCombat later) ---
    public void PlayPrimaryAttackAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(PrimaryAttackTriggerHash);
    }
    public void SetCombatState(CombatState state, bool isAiming = false)
    {
        if (animator == null) return;
        // Implement logic to play specific attack animations or set aiming boolean
        // This will be fleshed out when PlayerCombat is implemented
        animator.SetBool(IsAimingHash, isAiming); // Useful for turning on/off aimed locomotion blend tree
    }

    // --- Skill States (For PlayerSkillController later) ---
    public void PlaySkillCastAnimation(SkillState skill)
    {
        if (animator == null) return;
        animator.SetTrigger(SkillCastTriggerHash);
        // You might use an int parameter or separate triggers for specific skills
    }

    // --- Other States ---
    public void PlayDeathAnimation() { /* Implementation later */ }
    public void PlayInteractionAnimation() { /* Implementation later */ }
}