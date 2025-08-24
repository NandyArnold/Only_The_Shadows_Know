using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveableEntityRegistry : MonoBehaviour, IResettable
{
    public static SaveableEntityRegistry Instance { get; private set; }

    private readonly Dictionary<string, ISaveable> _registry = new Dictionary<string, ISaveable>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       
        Debug.Log("<color=lime>--- SaveableEntityRegistry AWAKE ---</color>");
    }

    public bool Register(ISaveable entity)
    {
        if (entity == null || string.IsNullOrEmpty(entity.UniqueID))
        {
            string reason = entity == null ? "Entity was null" : "UniqueID was null or empty";
            Debug.LogWarning($"<color=orange>[Registry]</color> SKIPPED registration for {((MonoBehaviour)entity)?.name ?? "NULL ENTITY"}. Reason: {reason}");
            return false; 
        }
        if (_registry.ContainsKey(entity.UniqueID))
        {
            // If a duplicate is found, log a detailed error and reject the new one.
            // This is the safeguard that will stop your duplication bug at the source.
            //Debug.LogError($"<color=red>[Registry] DUPLICATE ID ERROR:</color> " +
            //               $"Attempted to register '{((MonoBehaviour)entity).name}' " +
            //               $"with ID '{entity.UniqueID}', but this ID is already registered to '{((MonoBehaviour)_registry[entity.UniqueID]).name}'. " +
            //               $"Registration of the new object was rejected.",
            //               ((MonoBehaviour)entity).gameObject);
            return true;
        }
        // --- END OF UPGRADE ---

        _registry[entity.UniqueID] = entity;
        return true;

    }

    public void Unregister(ISaveable entity)
    {
        if (entity != null && !string.IsNullOrEmpty(entity.UniqueID) && _registry.ContainsKey(entity.UniqueID))
        {
            _registry.Remove(entity.UniqueID);
            //Debug.Log($"<color=orange>[Registry]</color> Unregistered: {((MonoBehaviour)entity).name} (ID: {entity.UniqueID}) | Total: {_registry.Count}");
        }
    }

    public IEnumerable<ISaveable> GetSaveableEntities()
    {
        return _registry.Values.ToList(); // Use ToList() for a safe copy
    }

    public ISaveable GetEntity(string id)
    {
        _registry.TryGetValue(id, out ISaveable entity);
       
        return entity;
    }

    public void ClearRegistry()
    {
       
        _registry.Clear();
        Debug.Log("<color=red>Saveable Entity Registry has been cleared.</color>");
    }
    public void ResetState()
    {
        ClearRegistry();
    }

}