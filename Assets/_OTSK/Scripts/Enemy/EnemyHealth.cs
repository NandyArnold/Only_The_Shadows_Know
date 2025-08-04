// EnemyHealth.cs - UPGRADED with OnDinamaged event
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    public float CurrentHealth => _currentHealth;
    private float _currentHealth;
    private bool _isDead = false;

    // The new event that EnemyAI will listen to.
    public event Action<GameObject> OnDamaged; // For AI logic
    public event Action<float, float> OnHealthChanged; // For UI
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
        OnHealthChanged?.Invoke(_currentHealth, maxHealth); // Notify UI about health change.

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

    public void SetHealth(float healthValue)
    {
        _currentHealth = healthValue;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        if (_currentHealth <= 0)
        {
            // If loaded health is zero, make sure the enemy is already dead.
            _isDead = true;
            gameObject.SetActive(false); // Instantly hide it.
        }
    }
}