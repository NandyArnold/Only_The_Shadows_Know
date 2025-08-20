// Create this new script, ChargeManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChargeManager : MonoBehaviour, ISaveable
{

    [SerializeField]
    private ChargeableItemRegistrySO itemRegistry;

    [System.Serializable]
    public struct ChargeSaveData
    {
        public List<string> itemNames; 
        public List<int> itemValues;
    }

    public string UniqueID => GetComponent<UniqueID>().ID;

    private int _infiniteChargeRequesters = 0;
    // Event to notify the UI when a charge count changes
    public event Action<ChargeableItemSO, int> OnChargeCountChanged;

    private Dictionary<ChargeableItemSO, int> _chargeCounts = new Dictionary<ChargeableItemSO, int>();

    private void Start()
    {
        Debug.Log($"--- ChargeManager.Start() called for {gameObject.name} ---");
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Register(this);
        }
        else
        {
            Debug.LogError($"Could not register {name}, SaveableEntityRegistry.Instance is null!");
        }
    }

    private void OnDestroy()
    {
        // Use OnDestroy for unregistering. It's safer than OnDisable for objects that get deactivated.
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Unregister(this);
        }
    }
    public void AddCharges(ChargeableItemSO item, int amount)
    {
        if (!_chargeCounts.ContainsKey(item))
        {
            _chargeCounts[item] = 0;
        }
        _chargeCounts[item] = Mathf.Min(_chargeCounts[item] + amount, item.maxCharges);
        OnChargeCountChanged?.Invoke(item, _chargeCounts[item]);
    }

    public bool HasCharges(ChargeableItemSO item, int amount = 1)
    {
        // If anyone has requested infinite charges, we always have them.
        if (_infiniteChargeRequesters > 0) return true;

        return _chargeCounts.ContainsKey(item) && _chargeCounts[item] >= amount;
    }

    public bool ConsumeCharge(ChargeableItemSO item, int amount = 1)
    {
        // If infinite charges are requested, do nothing but report success.
        if (_infiniteChargeRequesters > 0) return true;

        if (HasCharges(item, amount))
        {
            _chargeCounts[item] -= amount;
            OnChargeCountChanged?.Invoke(item, _chargeCounts[item]);
            return true;
        }
        return false;
    }

    public int GetChargeCount(ChargeableItemSO item)
    {
        _chargeCounts.TryGetValue(item, out int count);
        return count;
    }

    public void SetInfiniteCharges(bool hasInfiniteCharges)
    {
        if (hasInfiniteCharges)
        {
            _infiniteChargeRequesters++;
        }
        else
        {
            _infiniteChargeRequesters--;
        }
        _infiniteChargeRequesters = Mathf.Max(0, _infiniteChargeRequesters);
    }

    public object CaptureState()
    {
        return new ChargeSaveData
        {
            // Convert the dictionary keys (ScriptableObjects) to a list of their names (strings)
            itemNames = _chargeCounts.Keys.Select(item => item.name).ToList(),
            itemValues = _chargeCounts.Values.ToList()
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (ChargeSaveData)state;
        _chargeCounts.Clear();

        Debug.Log("<color=magenta>[ChargeManager]</color> RestoreState called. Found " + saveData.itemNames.Count + " item types in save file.");

        for (int i = 0; i < saveData.itemNames.Count; i++)
        {
            string itemName = saveData.itemNames[i];
            int value = saveData.itemValues[i];

            Debug.Log($"<color=magenta>[ChargeManager]</color> Loading item '{itemName}' with value '{value}'.");

            ChargeableItemSO item = itemRegistry.GetItem(saveData.itemNames[i]);
            if (item != null)
            {
                // Make sure you are using the value from the loop here
                _chargeCounts[item] = value;

                // This event tells the HUD to update its text
                OnChargeCountChanged?.Invoke(item, value);
                Debug.Log($"<color=green>[ChargeManager]</color> Successfully restored {item.name}. Firing UI update event with value {value}.");
            }
            else
            {
                Debug.LogWarning($"<color=orange>[ChargeManager]</color> Could not find item '{itemName}' in the Item Registry.");
            }
        }
    }
}