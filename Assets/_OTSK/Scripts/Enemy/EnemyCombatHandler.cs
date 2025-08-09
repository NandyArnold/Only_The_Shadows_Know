// In EnemyCombatHandler.cs
using UnityEngine;

public class EnemyCombatHandler : MonoBehaviour
{
    private EnemyAnimationController _animController;
    private PlayerStats _playerStats;
    private EnemyConfigSO _config;

    private void Awake()
    {
        _animController = GetComponent<EnemyAnimationController>();
        _config = GetComponent<Enemy>().Config;

        // Find the player's stats component when the enemy wakes up
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _playerStats = GameManager.Instance.Player.GetComponent<PlayerStats>();
        }
    }

    public void PerformAttack()
    {
        _animController.PlayPrimaryAttackAnimation();

        // After the animation trigger, deal damage to the player
        if (_playerStats != null)
        {
            // We can add a delay here later to sync with the animation swing
            _playerStats.TakeDamage(_config.attackDamage);
        }
    }
}