// EnemyAnimationController.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;

    private readonly int dieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayDeathAnimation()
    {
        _animator.SetTrigger(dieTriggerHash);
    }
}