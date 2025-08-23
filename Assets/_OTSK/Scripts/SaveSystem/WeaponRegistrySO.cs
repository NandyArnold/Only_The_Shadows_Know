using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "WeaponRegistry", menuName = "OTSK/Registries/Weapon Registry")]
public class WeaponRegistrySO : ScriptableObject
{
    [SerializeField] private List<WeaponSO> weapons;
    private Dictionary<string, WeaponSO> _lookup;

    public WeaponSO GetWeapon(string weaponName)
    {
        if (_lookup == null)
        {
            _lookup = weapons.ToDictionary(weapon => weapon.name);
        }
        _lookup.TryGetValue(weaponName, out WeaponSO weapon);
        return weapon;
    }
    public bool IsEmpty() => weapons == null || weapons.Count == 0;
    public void Reset()
    {
        // This will force the dictionary to be rebuilt on the next GetConfig call.
        _lookup = null;
    }
}