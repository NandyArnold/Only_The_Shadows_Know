using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    // This list will be configured in the Inspector
    [SerializeField] private List<VFXMapping> revealVFXMappings;
    [SerializeField] private GameObject riftPlaceVFXPrefab; 
    [SerializeField] private GameObject riftTeleportVFXPrefab;
    [SerializeField] private GameObject scryingCastVFXPrefab;

    // We use a dictionary for fast lookups at runtime
    private Dictionary<RevealableType, GameObject> _vfxDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Populate the dictionary from the list for efficient access
        _vfxDictionary = revealVFXMappings.ToDictionary(x => x.type, x => x.vfxPrefab);
    }

    // The method now takes a type to determine which VFX to play
    public GameObject GetRevealEffect(RevealableType type, Vector3 position)
    {
        if (_vfxDictionary.TryGetValue(type, out GameObject prefabToSpawn))
        {
            if (prefabToSpawn != null)
            {
                return Instantiate(prefabToSpawn, position, Quaternion.identity);
            }
        }
        return null;
    }
    public GameObject PlayRiftPlaceEffect(Vector3 position, Transform parent = null)
    {
        if (riftPlaceVFXPrefab != null)
        {
            // Use the Instantiate overload that takes a parent
            return Instantiate(riftPlaceVFXPrefab, position, Quaternion.identity, parent);
        }
        return null;
    }

    // NEW method for teleporting to a rift
    public GameObject PlayRiftTeleportEffect(Vector3 position, Transform parent = null)
    {
        if (riftTeleportVFXPrefab != null)
        {
            return Instantiate(riftTeleportVFXPrefab, position, Quaternion.identity, parent);
        }
        return null;
    }

    public GameObject PlayScryingCastEffect(Vector3 position, Transform parent)
    {
        if (scryingCastVFXPrefab != null)
        {
            return Instantiate(scryingCastVFXPrefab, position, Quaternion.identity, parent);
        }
        return null;
    }
}