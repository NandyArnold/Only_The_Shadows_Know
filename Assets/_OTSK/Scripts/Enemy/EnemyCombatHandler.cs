// In EnemyCombatHandler.cs
using System.Collections.Generic;
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

    private bool _damageWindowIsOpen = false;
    private readonly List<PlayerStats> _playersHitThisSwing = new List<PlayerStats>();

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

    public void OpenDamageWindow()
    {
        _playersHitThisSwing.Clear(); // Clear the list of who we've hit
        _damageWindowIsOpen = true;
    }

    // NEW: This is called by the SECOND animation event to close the window.
    public void CloseDamageWindow()
    {
        _damageWindowIsOpen = false;
    }

    // NEW: We now check for damage every frame inside Update.
    private void Update()
    {
        // Only check for hits if the damage window is open.
        if (_damageWindowIsOpen)
        {
            DealDamage();
        }
    }
    public void PerformAttack()
    {
        _animController.PlayPrimaryAttackAnimation();
    }

    private void DealDamage()
    {
        if (_playerStats == null || weaponHitbox == null) return;

        Collider[] hits = Physics.OverlapBox(weaponHitbox.position, hitboxSize / 2, weaponHitbox.rotation, hittableLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == _playerStats.gameObject)
            {
                // Check if we have ALREADY hit the player during this swing.
                if (!_playersHitThisSwing.Contains(_playerStats))
                {
                    _playerStats.TakeDamage(_config.attackDamage);
                    _playersHitThisSwing.Add(_playerStats); // Add to the list to prevent multi-hits
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (weaponHitbox == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = weaponHitbox.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
    }

    public void CancelAttack()
    {
        _damageWindowIsOpen = false;
    }
}