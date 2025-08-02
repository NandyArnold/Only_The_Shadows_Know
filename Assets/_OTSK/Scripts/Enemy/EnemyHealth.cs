// EnemyHealth.cs - UPGRADED with OnDinamaged event
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    private float _currentHealth;
    private bool _isDead = false;

    // The new event that EnemyAI will listen to.
    public event Action<GameObject> OnDamaged;
    public event Action OnDied;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    // The TakeDamage method now requires an "attacker" so it can broadcast who did the damage.
    public void TakeDamage(float damageAmount, GameObject attacker)
    {
        if (_isDead) return;

        _currentHealth -= damageAmount;
        OnDamaged?.Invoke(attacker); // Fire the event to notify the AI.

        Debug.Log($"<color=orange>{gameObject.name} took {damageAmount} damage from {attacker.name}!</color>");

        if (_currentHealth <= 0)
        {
            _isDead = true;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        OnDied?.Invoke();
    }
}