// EnemyHealth.cs - UPGRADED
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    // NEW: An event that fires when damaged, passing the attacker.
    public event Action<GameObject> OnDamaged;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    // UPDATED: The method now requires an "attacker" parameter.
    public void TakeDamage(float damageAmount, GameObject attacker)
    {
        _currentHealth -= damageAmount;
        OnDamaged?.Invoke(attacker); // Fire the event

        Debug.Log($"<color=orange>{gameObject.name} took {damageAmount} damage from {attacker.name}!</color>");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        Destroy(gameObject);
    }
}