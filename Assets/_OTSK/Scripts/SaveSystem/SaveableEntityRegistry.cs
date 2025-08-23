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

    public void Register(ISaveable entity)
    {
        //Debug.Log($"[Registry] Attempting to register '{((MonoBehaviour)entity)?.name ?? "NULL ENTITY"}'...");
        if (entity != null && !string.IsNullOrEmpty(entity.UniqueID))
        {
            _registry[entity.UniqueID] = entity;
            //Debug.Log($"<color=cyan>[Registry]</color> Registered: {((MonoBehaviour)entity).name} (ID: {entity.UniqueID}) | Total: {_registry.Count}");
        }
        else
        {
            // Log that the conditions FAILED and why
            string reason = entity == null ? "Entity was null" : "UniqueID was null or empty";
            Debug.LogWarning($"<color=orange>[Registry]</color> SKIPPED registration for {((MonoBehaviour)entity)?.name ?? "NULL ENTITY"}. Reason: {reason}");
        }
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