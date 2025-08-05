// EnemyHealth.cs - UPGRADED with OnDinamaged event
using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    private float _currentHealth;
    private bool _isDead = false;

    // The new event that EnemyAI will listen to.
    public event Action<GameObject> OnDamaged; // For AI logic
    public event Action<float, float> OnHealthChanged; // For UI
    public event Action OnDied;

    private EnemyResistances _resistances;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _resistances = GetComponent<EnemyResistances>();
    }

    // The TakeDamage method now requires an "attacker" so it can broadcast who did the damage.
    public void TakeDamage(List<DamageInstance> damageInstances, GameObject attacker)
    {
        if (_isDead) return;

        float totalDamage = 0;
        // Loop through each piece of damage in the attack
        foreach (var instance in damageInstances)
        {
            // Ask the DamageType to calculate its damage, and then apply resistances
            float damage = instance.DamageType.CalculateDamage(instance.Value, this.gameObject);
            float multiplier = _resistances != null ? _resistances.GetMultiplier(instance.DamageType) : 1f;
            totalDamage += damage * multiplier;
        }

        _currentHealth -= totalDamage;
        OnDamaged?.Invoke(attacker);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        Debug.Log($"<color=orange>{gameObject.name} took {totalDamage} total damage!</color>");

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