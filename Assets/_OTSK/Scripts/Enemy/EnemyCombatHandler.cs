// In EnemyCombatHandler.cs
using UnityEngine;

public class EnemyCombatHandler : MonoBehaviour
{
    private EnemyAnimationController _animController;
    private PlayerStats _playerStats;
    private EnemyConfigSO _config;

    [Header("Hitbox")]
    [Tooltip("An empty GameObject on the weapon bone to define the center of the attack.")]
    [SerializeField] private Transform weaponHitbox;
    [SerializeField] private Vector3 hitboxSize = new Vector3(0.5f, 1.5f, 0.5f);
    [SerializeField] private LayerMask hittableLayers;
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
    }

    public void DealDamage()
    {
        if (_playerStats == null || weaponHitbox == null) return;

        Collider[] hits = Physics.OverlapBox(weaponHitbox.position, hitboxSize / 2, weaponHitbox.rotation, hittableLayers);

        foreach (var hit in hits)
        {
            // Check if we hit the player
            if (hit.gameObject == _playerStats.gameObject)
            {
                Debug.Log($"<color=red>Player Hit!</color> Dealing {_config.attackDamage} damage.");
                _playerStats.TakeDamage(_config.attackDamage);
                return; // Only hit the player once per swing
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponHitbox == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = weaponHitbox.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
    }

}