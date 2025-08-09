// PlayerHealthManaNoise.cs

using System;
using UnityEngine;

/// <summary>
/// Manages the player's core statistics: Health, Mana, and Noise Level.
/// It handles modifications to these stats and broadcasts events when they change.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
   

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
   

    // --- Public Properties ---
    public float CurrentHealth { get; private set; }
    public float CurrentMana { get; private set; }
    
    //public float CurrentNoise => _currentNoiseLevel;

    // --- Public Events ---

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
   
    private void Awake()
    {
       
        CurrentHealth = maxHealth;
        CurrentMana = maxMana;
      
    }

    // --- Public Methods for Modifying Stats ---

    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        Debug.Log($"Player took {amount} damage. Current Health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public bool ConsumeMana(float amount)
    {
        if (CurrentMana >= amount)
        {
            CurrentMana = Mathf.Clamp(CurrentMana - amount, 0, maxMana);
            OnManaChanged?.Invoke(CurrentMana, maxMana);
            Debug.Log($"Player consumed {amount} mana. Current Mana: {CurrentMana}");
            return true; // Success
        }

        Debug.Log("Not enough mana!");
        return false; // Failure
    }

    public void RestoreHealth(float amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Debug.Log($"Player restored {amount} health. Current Health: {CurrentHealth}");
    }

    public void RestoreMana(float amount)
    {
        CurrentMana = Mathf.Clamp(CurrentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
        Debug.Log($"Player restored {amount} mana. Current Mana: {CurrentMana}");
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died.");
        // TODO: Trigger death animation, show death menu, etc.
    }
    public void RestoreStats(float health, float mana)
    {
        CurrentHealth = health;
        CurrentMana = mana;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

}