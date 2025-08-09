// EnemyCombatHandler.cs
using UnityEngine;

public class EnemyCombatHandler : MonoBehaviour
{
    private EnemyAnimationController _animController;

    private void Awake()
    {
        _animController = GetComponent<EnemyAnimationController>();
    }

    public void PerformAttack()
    {
        _animController.PlayPrimaryAttackAnimation();
        //Debug.Log("Enemy Attacks!");
    }
}