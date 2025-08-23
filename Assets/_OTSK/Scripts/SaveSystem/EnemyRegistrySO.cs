using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "EnemyRegistry", menuName = "OTSK/Registries/Enemy Registry")]
public class EnemyRegistrySO : ScriptableObject
{
    [SerializeField] private List<EnemyConfigSO> enemyConfigs;

    private Dictionary<string, EnemyConfigSO> _lookup;

    public EnemyConfigSO GetConfig(string configName)
    {
        if (_lookup == null) // Build the lookup dictionary on first request
        {
            _lookup = enemyConfigs.ToDictionary(config => config.name);
        }

        _lookup.TryGetValue(configName, out EnemyConfigSO config);
        return config;
    }
    public bool IsEmpty() => enemyConfigs == null || enemyConfigs.Count == 0;
    public void Reset()
    {
        // This will force the dictionary to be rebuilt on the next GetConfig call.
        _lookup = null;
    }
}