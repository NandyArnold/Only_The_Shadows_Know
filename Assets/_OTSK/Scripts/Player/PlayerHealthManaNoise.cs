// PlayerHealthManaNoise.cs

using System;
using UnityEngine;

/// <summary>
/// Manages the player's core statistics: Health, Mana, and Noise Level.
/// It handles modifications to these stats and broadcasts events when they change.
/// </summary>
public class PlayerHealthManaNoise : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    private float _currentMana;

    [Header("Noise Settings")]
    [SerializeField] private float maxNoise = 100f;
    private float _currentNoiseLevel;

    // --- Public Properties ---
    public float CurrentHealth => _currentHealth;
    public float CurrentMana => _currentMana;
    public float CurrentNoise => _currentNoiseLevel;

    // --- Public Events ---
    // The UI will subscribe to these events to update itself.
    // Passes (currentValue, maxValue) for easy percentage calculation.
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    public event Action<float, float> OnNoiseChanged;

    private void Awake()
    {
        // Initialize stats at the beginning.
        _currentHealth = maxHealth;
        _currentMana = maxMana;
        _currentNoiseLevel = 0f;
    }

    // --- Public Methods for Modifying Stats ---

    public void TakeDamage(float amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        Debug.Log($"Player took {amount} damage. Current Health: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public bool ConsumeMana(float amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana = Mathf.Clamp(_currentMana - amount, 0, maxMana);
            OnManaChanged?.Invoke(_currentMana, maxMana);
            Debug.Log($"Player consumed {amount} mana. Current Mana: {_currentMana}");
            return true; // Success
        }

        Debug.Log("Not enough mana!");
        return false; // Failure
    }

    public void RestoreHealth(float amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        Debug.Log($"Player restored {amount} health. Current Health: {_currentHealth}");
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Clamp(_currentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(_currentMana, maxMana);
        Debug.Log($"Player restored {amount} mana. Current Mana: {_currentMana}");
    }

    public void GenerateNoise(float amount)
    {
        _currentNoiseLevel = Mathf.Clamp(_currentNoiseLevel + amount, 0, maxNoise);
        OnNoiseChanged?.Invoke(_currentNoiseLevel, maxNoise);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.BroadcastSound(transform.position, amount, gameObject);
        }
        // We'll also want a way for noise to decay over time, but we can add that later.
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died.");
        // TODO: Trigger death animation, show death menu, etc.
    }

}