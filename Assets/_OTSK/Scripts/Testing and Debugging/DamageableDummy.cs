using UnityEngine;

public class DamageableDummy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    // A simple method to take damage, without any complex payload.
    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        Debug.Log($"<color=yellow>DUMMY TOOK {damageAmount} DAMAGE.</color> Current Health: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0)
        {
            Debug.Log("<color=red>DUMMY DESTROYED.</color>");
            // For simplicity, we can just disable the dummy's renderer.
            GetComponent<MeshRenderer>().enabled = false;
        }
    }
}