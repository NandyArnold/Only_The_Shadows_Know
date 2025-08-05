using UnityEngine;

public class AnimancyAnimation : MonoBehaviour, IWeaponAnimation
{
    [SerializeField] private Animator animator;

    // We can add specific trigger hashes here later
    // private readonly int rangedCastTrigger = Animator.StringToHash("Animancy_RangedCast");
    // private readonly int meleeCastTrigger = Animator.StringToHash("Animancy_MeleeCast");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void PlayPrimaryAttack()
    {
        // TODO: Trigger a ranged casting animation
        Debug.Log("Animancy Animation: Playing Ranged Attack");
    }

    public void PlaySecondaryAttack()
    {
        // TODO: Trigger a melee casting animation
        Debug.Log("Animancy Animation: Playing Melee Attack");
    }
}