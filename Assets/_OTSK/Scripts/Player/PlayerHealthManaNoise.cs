using UnityEngine;

public class PlayerHealthManaNoise : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    [SerializeField] private float maxMana = 100f;
    public float currentMana;

    [SerializeField] private float maxNoiseLevel = 100f;
    public float currentNoiseLevel;

    // Events will be added later in Day 2 for UI updates etc.
    // public event Action<float> OnHealthChanged;
    // public event Action<float> OnManaChanged;
    // public event Action<float> OnNoiseChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentNoiseLevel = 0f; // Start with no noise
    }

    // Placeholder methods for now
    public void TakeDamage(float amount) { /* ... */ }
    public void RestoreHealth(float amount) { /* ... */ }
    public void ConsumeMana(float amount) { /* ... */ }
    public void RestoreMana(float amount) { /* ... */ }
    public void GenerateNoise(float amount) { /* ... */ }
    public void SetNoiseModifier(float modifier) { /* ... */ }
}
