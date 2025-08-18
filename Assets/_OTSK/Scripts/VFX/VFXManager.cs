using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    // This list will be configured in the Inspector
    //[SerializeField] private List<VFXMapping> revealVFXMappings;
    [SerializeField] private GameObject riftPlaceVFXPrefab; 
    [SerializeField] private GameObject riftTeleportVFXPrefab;
    [SerializeField] private GameObject scryingCastVFXPrefab;

    [Header("Reveal VFX")]
    [SerializeField] private Material enemyRevealMaterial;
    [SerializeField] private Material interactableRevealMaterial;
    [SerializeField] private Material bossRevealMaterial;
    [SerializeField] private Material casterRevealMaterial;
    [SerializeField] private Transform worldMarkersCanvas;
    //[SerializeField] private GameObject bossIconUIPrefab;

    //// We use a dictionary for fast lookups at runtime
    //private Dictionary<RevealableType, GameObject> _vfxDictionary;
  

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Populate the dictionary from the list for efficient access
        //_vfxDictionary = revealVFXMappings.ToDictionary(x => x.type, x => x.vfxPrefab);
    }

    // The method now takes a type to determine which VFX to play
    //public GameObject GetRevealEffect(RevealableType type, Vector3 position)
    //{
    //    if (_vfxDictionary.TryGetValue(type, out GameObject prefabToSpawn))
    //    {
    //        if (prefabToSpawn != null)
    //        {
    //            return Instantiate(prefabToSpawn, position, Quaternion.identity);
    //        }
    //    }
    //    return null;
    //}
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

    public void PlayInstaKillEffect()
    {
        Debug.Log("Playing InstaKill VFX" );
        //GameObject instaKillVFX = GetRevealEffect(RevealableType.InstaKill, position);
        //if (instaKillVFX != null)
        //{
        //    // Optionally, you can add a cleanup mechanism to destroy the effect after some time
        //    Destroy(instaKillVFX, 2f); // Adjust duration as needed
        //}
    }

    public List<GameObject> CreateOutlineEffect(RevealableEntity entity)
    {
        var outlineObjects = new List<GameObject>();
        Material[] outlineMaterials = GetOutlineMaterials(entity.Type);

        // If we don't have a valid material for this type, do nothing.
        if (outlineMaterials == null) return null;

        // --- 1. Handle Skinned Meshes (for characters) ---
        SkinnedMeshRenderer[] originalSkinnedMeshes = entity.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var originalMesh in originalSkinnedMeshes)
        {
            GameObject outlineObject = new GameObject(entity.name + "_Outline");
            outlineObject.transform.SetParent(originalMesh.transform, false);

            SkinnedMeshRenderer newMesh = outlineObject.AddComponent<SkinnedMeshRenderer>();
            newMesh.sharedMesh = originalMesh.sharedMesh;
            newMesh.sharedMaterials = outlineMaterials;
            newMesh.bones = originalMesh.bones;
            newMesh.rootBone = originalMesh.rootBone;
            outlineObjects.Add(outlineObject);
        }

        // --- 2. Handle Static Meshes (for objects like Alarm Panels) ---
        MeshRenderer[] originalStaticMeshes = entity.GetComponentsInChildren<MeshRenderer>();
        foreach (var originalMesh in originalStaticMeshes)
        {
            GameObject outlineObject = new GameObject(entity.name + "_Outline");
            outlineObject.transform.SetParent(originalMesh.transform, false);

            // For static meshes, we need a MeshFilter and a MeshRenderer
            MeshFilter newFilter = outlineObject.AddComponent<MeshFilter>();
            newFilter.sharedMesh = originalMesh.GetComponent<MeshFilter>().sharedMesh;

            MeshRenderer newRenderer = outlineObject.AddComponent<MeshRenderer>();
            newRenderer.sharedMaterials = outlineMaterials;
            outlineObjects.Add(outlineObject);
        }

        // --- 3. Handle Boss Icons ---
        //if (entity.Type == RevealableType.Boss && bossIconUIPrefab != null && worldMarkersCanvas != null)
        //{
        //    // Find a head socket or just use the entity's transform
        //    Transform headSocket = entity.transform.Find("HeadSocket") ?? entity.transform;

        //    // Instantiate the UI prefab and parent it to the dedicated canvas
        //    GameObject iconInstance = Instantiate(bossIconUIPrefab, worldMarkersCanvas);

        //    // Tell the new UI icon which 3D target to follow
        //    if (iconInstance.TryGetComponent<BossIconUI>(out var iconUI))
        //    {
        //        iconUI.targetToFollow = headSocket;
        //    }

        //    // Add the UI instance to the list to be managed and destroyed later
        //    outlineObjects.Add(iconInstance);
        //}

        return outlineObjects;
    }

    // A helper to get the correct material based on type
    private Material[] GetOutlineMaterials(RevealableType type)
    {
        switch (type)
        {
            case RevealableType.Normal:
                return new Material[] { enemyRevealMaterial };
            case RevealableType.Boss:
                return new Material[] { bossRevealMaterial };
            case RevealableType.HiddenItem:
                return new Material[] { interactableRevealMaterial };
            case RevealableType.Caster: 
                return new Material[] { casterRevealMaterial };
            default:
                return null;
        }
    }
}