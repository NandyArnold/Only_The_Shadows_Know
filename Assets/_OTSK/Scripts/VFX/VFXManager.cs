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
    [SerializeField] private Material hiddenItemeRevealMaterial;
    [SerializeField] private Material manaAdditionRevealMaterial;
    [SerializeField] private Material arrowAdditionRevealMaterial;
    [SerializeField] private Material alarmPanelRevealMaterial;
    [SerializeField] private Material normalRevealMaterial;
    [SerializeField] private Material priorityEnemyRevealMaterial;
    [SerializeField] private Material summonerRevealMaterial;
    [SerializeField] private Material eliteSummonerRevealMaterial;
    [SerializeField] private Material instakillRevealMaterial;
    [SerializeField] private Material undeadRevealMaterial;
    [SerializeField] private Material casterRevealMaterial;
    [SerializeField] private Material eliteRevealMaterial;
    [SerializeField] private Material bossRevealMaterial;

    [SerializeField] private Transform worldMarkersCanvas;

    [SerializeField] private GameObject endwalkerAuraPrefab;
    private List<GameObject> _activeEndwalkerAuras = new List<GameObject>();
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

      

        return outlineObjects;
    }

    // A helper to get the correct material based on type
    private Material[] GetOutlineMaterials(RevealableType type)
    {
        switch (type)
        {
            case RevealableType.HiddenItem:
                return new Material[] { hiddenItemeRevealMaterial };
            case RevealableType.ManaAddition:
                return new Material[] { manaAdditionRevealMaterial };
            case RevealableType.ArrowAddition:
                return new Material[] { arrowAdditionRevealMaterial };
            case RevealableType.AlarmPanel:
                return new Material[] { alarmPanelRevealMaterial };
            case RevealableType.Normal:
                return new Material[] { normalRevealMaterial };
            case RevealableType.Priority:
                return new Material[] { priorityEnemyRevealMaterial };
            case RevealableType.Summoner:
                return new Material[] { summonerRevealMaterial };
            case RevealableType.EliteSummoner:
                return new Material[] { eliteSummonerRevealMaterial };
            case RevealableType.Instakill:
                return new Material[] { instakillRevealMaterial };
            case RevealableType.Undead:
                return new Material[] { undeadRevealMaterial };
            case RevealableType.Caster: 
                return new Material[] { casterRevealMaterial };
            case RevealableType.Elite:
                return new Material[] { eliteRevealMaterial };
            case RevealableType.Boss:
                return new Material[] { bossRevealMaterial };
            default:
                return null;
        }
    }

    public void PlayEndwalkerAura(Transform target)
    {
        if (endwalkerAuraPrefab == null) return;
        GameObject auraInstance = Instantiate(endwalkerAuraPrefab, target.position, target.rotation, target);
        _activeEndwalkerAuras.Add(auraInstance);
    }

    public void StopAllEndwalkerAuras()
    {
        foreach (var aura in _activeEndwalkerAuras)
        {
            if (aura != null) Destroy(aura);
        }
        _activeEndwalkerAuras.Clear();
    }
}