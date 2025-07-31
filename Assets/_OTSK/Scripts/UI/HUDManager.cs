// HUDManager.cs

using UnityEngine;
using UnityEngine.UI; // Required for UI components like Slider

/// <summary>
/// Manages the player's Heads-Up Display elements.
/// It listens to events from other systems to update the UI.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    private PlayerHealthManaNoise _playerStats;

    void Start()
    {
        // Wait for the player to be registered before trying to find it.
        // A more robust system might use an event from the GameManager.
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            InitializeHUD(GameManager.Instance.Player);
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when this object is destroyed.
        if (_playerStats != null)
        {
            _playerStats.OnHealthChanged -= UpdateHealthBar;
            _playerStats.OnManaChanged -= UpdateManaBar;
        }
    }

    private void InitializeHUD(PlayerController player)
    {
        _playerStats = player.GetComponent<PlayerHealthManaNoise>();
        if (_playerStats != null)
        {
            // Subscribe to the events
            _playerStats.OnHealthChanged += UpdateHealthBar;
            _playerStats.OnManaChanged += UpdateManaBar;

            // Set the initial values of the bars
            UpdateHealthBar(_playerStats.CurrentHealth, 100f); // Assuming max health is 100
            UpdateManaBar(_playerStats.CurrentMana, 100f);   // Assuming max mana is 100
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaSlider != null)
        {
            manaSlider.value = currentMana / maxMana;
        }
    }
}
