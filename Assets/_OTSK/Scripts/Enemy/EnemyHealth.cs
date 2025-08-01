// EnemyHealth.cs

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;
    private float _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    // This is the public method our player's attack will call.
    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        Debug.Log($"<color=orange>{gameObject.name} took {damageAmount} damage! Health is now {_currentHealth}/{maxHealth}</color>");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");
        // For now, we'll just destroy the GameObject to show it's dead.
        Destroy(gameObject);
    }
}