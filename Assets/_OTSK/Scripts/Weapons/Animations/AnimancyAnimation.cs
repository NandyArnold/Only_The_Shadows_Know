// In AnimancyAnimation.cs
using UnityEngine;

public class AnimancyAnimation : MonoBehaviour, IWeaponAnimation
{
    [SerializeField] private Animator animator;

    // NEW: Specific trigger hashes
    private readonly int rangedCastTrigger = Animator.StringToHash("Animancy_Ranged");
    private readonly int meleeCastTrigger = Animator.StringToHash("Animancy_Melee");
    private readonly int deathZoneCastTrigger = Animator.StringToHash("Animancy_DeathZone"); 



    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void PlayPrimaryAttack() => animator.SetTrigger(rangedCastTrigger);
    public void PlaySecondaryAttack() => animator.SetTrigger(meleeCastTrigger);
    public void PlayTertiaryAttack() => animator.SetTrigger(deathZoneCastTrigger);
}