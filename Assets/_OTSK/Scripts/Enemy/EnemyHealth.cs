// EnemyHealth.cs - UPGRADED with OnDinamaged event
using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;

    private float _currentHealth;
    private float _maxHealth = 100f;

    private bool _isDead = false;

    // The new event that EnemyAI will listen to.
    public event Action<GameObject> OnDamaged; // For AI logic
    public event Action<float, float> OnHealthChanged; // For UI
    public event Action OnDied;

    private EnemyResistances _resistances;
    private EnemyConfigSO _config;

    private void Awake()
    {
        _config = GetComponent<Enemy>().Config;
        if (_config != null)
        {
            _currentHealth = _config.maxHealth;
        }
        _resistances = GetComponent<EnemyResistances>();

    }
    public void Initialize(EnemyConfigSO config)
    {
        _maxHealth = config.maxHealth;
        _currentHealth = _maxHealth;
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
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

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
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        if (_currentHealth <= 0)
        {
            // If loaded health is zero, make sure the enemy is already dead.
            _isDead = true;
            gameObject.SetActive(false); // Instantly hide it.
        }
    }
}