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
    public float MaxHealth => maxHealth;

    //public float CurrentNoise => _currentNoiseLevel;

    // --- Public Events ---

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    public event Action OnDamaged;
    public event Action OnDied;

    private PlayerAnimationController _animController;
    private PlayerController _playerController;
    private Invulnerability _invulnerability;



    private void Awake()
    {
       
        CurrentHealth = maxHealth;
        CurrentMana = maxMana;
        _animController = GetComponent<PlayerAnimationController>();
        _playerController = GetComponent<PlayerController>();
        _invulnerability = GetComponent<Invulnerability>();
    }

    // --- Public Methods for Modifying Stats ---

    public void TakeDamage(float amount)
    {
        if (_invulnerability != null && _invulnerability.IsInvulnerable)
        {
            OnDamaged?.Invoke(); // We still fire the event so animations can play
            _animController?.PlayTakeDamageAnimation();
            return; // No damage is taken
        }
        if (CurrentHealth <= 0) return; // Don't take damage if already dead

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnDamaged?.Invoke();
        _animController?.PlayTakeDamageAnimation(); // Trigger damage animation


        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke(); // Announce that the player has died
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
        // Add the amount, but don't go over the maximum
        CurrentMana = Mathf.Min(CurrentMana + amount, maxMana);
        OnManaChanged?.Invoke(CurrentMana, maxMana);
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

    public void Revive()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        _playerController.SetIsDead(false); // We need to add this method to PlayerController
        _animController.PlayReviveAnimation(); // We need to add this method to PlayerAnimationController
    }

    public void RestoreHealthToFull()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void RestoreManaToFull()
    {
        CurrentMana = maxMana;
        OnManaChanged?.Invoke(CurrentMana, maxMana);
    }

    public void ReviveOnLoad()
    {
        // This version does NOT restore health, assuming it's already been set from the save file.
        _playerController.SetIsDead(false);
        _animController.PlayReviveAnimation();
    }

}