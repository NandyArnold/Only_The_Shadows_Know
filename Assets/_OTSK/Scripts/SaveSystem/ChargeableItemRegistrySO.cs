using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ChargeableItemRegistry", menuName = "OTSK/Registries/Chargeable Item Registry")]
public class ChargeableItemRegistrySO : ScriptableObject
{
    [SerializeField] private List<ChargeableItemSO> chargeableItems;

    private Dictionary<string, ChargeableItemSO> _lookup;

    public ChargeableItemSO GetItem(string itemName)
    {
        if (_lookup == null) // Build the lookup dictionary on first request
        {
            _lookup = chargeableItems.ToDictionary(item => item.name);
        }

        _lookup.TryGetValue(itemName, out ChargeableItemSO item);
        return item;
    }

    public bool IsEmpty() => chargeableItems == null || chargeableItems.Count == 0;
    public void Reset()
    {
        // This will force the dictionary to be rebuilt on the next GetConfig call.
        _lookup = null;
    }
}