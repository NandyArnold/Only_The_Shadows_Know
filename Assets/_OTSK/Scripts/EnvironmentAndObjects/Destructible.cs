// In Destructible.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private DestructibleDataSO data; // Reference to its data

    [Header("UI")]
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private Transform statusBarAnchor;
    [SerializeField] private GameObject revealIconPrefab;

    public event Action OnDied;
    public event Action<float, float> OnHealthChanged;

    private float _currentHealth;
    private EnemyUIController _uiController;

    private void Awake()
    {
        _currentHealth = data.maxHealth;
        if (statusBarPrefab != null && statusBarAnchor != null)
        {
            GameObject statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, statusBarAnchor);
            _uiController = statusBarInstance.GetComponent<EnemyUIController>();

            // Subscribe the UI to this object's health changes
            if (_uiController != null)
            {
                this.OnHealthChanged += _uiController.UpdateHealth;
                // Set the initial state (full health, so it will be hidden)
                _uiController.UpdateHealth(_currentHealth, data.maxHealth);
                _uiController.InitializeRevealIcon(revealIconPrefab);
            }
        }
    }


    private void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (_uiController != null)
        {
            this.OnHealthChanged -= _uiController.UpdateHealth;
        }
    }


    // This method now accepts a full damage profile
    public void TakeDamage(List<DamageInstance> damageInstances, GameObject attacker)
    {
        float totalDamage = 0;
        foreach (var instance in damageInstances)
        {
            float multiplier = data.GetMultiplier(instance.DamageType);
            totalDamage += instance.Value * multiplier;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - totalDamage);

        // Announce that the health has changed
        OnHealthChanged?.Invoke(_currentHealth, data.maxHealth);

        if (_currentHealth <= 0)
        {
            OnDied?.Invoke();
            gameObject.SetActive(false);
        }
    }
}