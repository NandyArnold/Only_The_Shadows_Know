// In Destructible.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour, ISaveable
{

    [Serializable] 
    public struct DestructibleSaveData
    {
        public float currentHealth;
        public bool wasDestroyed;
    }

    [SerializeField] private DestructibleDataSO data; // Reference to its data

    [Header("UI")]
    [SerializeField] private GameObject statusBarPrefab;
    [SerializeField] private Transform statusBarAnchor;
    [SerializeField] private GameObject revealIconPrefab;

    public event Action OnDied;
    public event Action<float, float> OnHealthChanged;

    private float _currentHealth;
    private DestructibleUIController _uiController;
    private UniqueID _uniqueID;
    public string UniqueID => _uniqueID.ID;
    private void Awake()
    {
        _uniqueID = GetComponent<UniqueID>();
        _currentHealth = data.maxHealth;
        if (statusBarPrefab != null && statusBarAnchor != null)
        {
            GameObject statusBarInstance = Instantiate(statusBarPrefab, statusBarAnchor.position, statusBarAnchor.rotation, statusBarAnchor);
            _uiController = statusBarInstance.GetComponent<DestructibleUIController>();
            _uiController.InitializeRevealIcon(revealIconPrefab);

            // Subscribe the UI to this object's health changes
            if (_uiController != null)
            {
                this.OnHealthChanged += _uiController.UpdateHealth;
                // Set the initial state (full health, so it will be hidden)
                _uiController.UpdateHealth(_currentHealth, data.maxHealth);
            }
        }
    }


 

    public object CaptureState()
    {
        return new DestructibleSaveData
        {
            currentHealth = _currentHealth,
            wasDestroyed = !gameObject.activeSelf
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (DestructibleSaveData)state;
        _currentHealth = saveData.currentHealth;
        OnHealthChanged?.Invoke(_currentHealth, data.maxHealth);

        if (saveData.wasDestroyed)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start() 
    {
        SaveableEntityRegistry.Instance.Register(this);
        if (ObjectiveTargetRegistry.Instance != null && !string.IsNullOrEmpty(data.destructibleID))
        {
            ObjectiveTargetRegistry.Instance.RegisterTarget(data.destructibleID, this.transform);
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (_uiController != null)
        {
            this.OnHealthChanged -= _uiController.UpdateHealth;
        }
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Unregister(this);
        }
        if (ObjectiveTargetRegistry.Instance != null && data != null && !string.IsNullOrEmpty(data.destructibleID))
        {
            ObjectiveTargetRegistry.Instance.UnregisterTarget(data.destructibleID, this.transform);
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
            if (data.onDestroyedEvent != null && !string.IsNullOrEmpty(data.destructibleID))
            {
                data.onDestroyedEvent.Raise(data.destructibleID);
            }

            OnDied?.Invoke();
            gameObject.SetActive(false);
        }
    }
}