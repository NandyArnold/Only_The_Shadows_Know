// Create this new script, ChargeManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChargeManager : MonoBehaviour
{
    private int _infiniteChargeRequesters = 0;
    // Event to notify the UI when a charge count changes
    public event Action<ChargeableItemSO, int> OnChargeCountChanged;

    private Dictionary<ChargeableItemSO, int> _chargeCounts = new Dictionary<ChargeableItemSO, int>();

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
}