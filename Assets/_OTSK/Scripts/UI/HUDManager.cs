// HUDManager.cs - UPGRADED VERSION

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("The parent GameObject for the entire Player HUD.")]
    [SerializeField] private GameObject playerHUDContainer;

    [Header("Stat Bar References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    [Header("Objective UI")] // NEW
    [SerializeField]
    private TextMeshProUGUI objectiveText;

    private PlayerHealthManaNoise _playerStats;

    private void Start()
    {
        // Subscribe to the GameManager's state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        // The HUD starts hidden. The GameState change will enable it.
        if (playerHUDContainer != null) playerHUDContainer.SetActive(false);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged += UpdateObjectiveText;
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        // Hide text initially
        objectiveText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (_playerStats != null)
        {
            _playerStats.OnHealthChanged -= UpdateHealthBar;
            _playerStats.OnManaChanged -= UpdateManaBar;
        }

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged -= UpdateObjectiveText;
            ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }

    }

    private void HandleGameStateChanged(GameState newState)
    {
        // Show the HUD only when the state is Gameplay.
        if (playerHUDContainer != null)
        {
            playerHUDContainer.SetActive(newState == GameState.Gameplay);
        }

        // If we are entering gameplay for the first time, find the player and set up the bars.
        if (newState == GameState.Gameplay && _playerStats == null)
        {
            InitializeHUD(GameManager.Instance.Player);
        }
    }

    private void InitializeHUD(PlayerController player)
    {
        if (player == null) return;
        _playerStats = player.GetComponent<PlayerHealthManaNoise>();

        if (_playerStats != null)
        {
            _playerStats.OnHealthChanged += UpdateHealthBar;
            _playerStats.OnManaChanged += UpdateManaBar;

            UpdateHealthBar(_playerStats.CurrentHealth, 100f);
            UpdateManaBar(_playerStats.CurrentMana, 100f);
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

    private void UpdateObjectiveText(ObjectiveSO newObjective)
    {
        if (newObjective != null)
        {
            objectiveText.text = newObjective.objectiveDescription;
            objectiveText.gameObject.SetActive(true);
        }
        else
        {
            objectiveText.gameObject.SetActive(false);
        }
    }
    private void HandleLevelCompleted()
    {
        objectiveText.text = "All Objectives Complete!";
    }

}