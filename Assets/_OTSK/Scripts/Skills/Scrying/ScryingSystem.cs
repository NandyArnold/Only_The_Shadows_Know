// ScryingSystem.cs - Final Definitive Version

using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }
    public RenderTexture ScryingRenderTexture => scryingRenderTexture;
    [Header("Configuration")]
    [SerializeField] private RenderTexture scryingRenderTexture;

    private GameObject scryingCameraRigObject;
    //private CinemachineCamera scryingVCam;
    public CinemachineCamera ScryingVCam { get; private set; }
    public bool IsScryingDeployed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to the SceneLoader event to know when a new scene is ready.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoaded;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent issues.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        }
    }

    /// <summary>
    /// This is called by the SceneLoader after a new scene has finished loading.
    /// </summary>
    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        // We only search for the rig in gameplay scenes.
        if (sceneData.sceneType == SceneType.Gameplay)
        {
            StartCoroutine(FindSceneComponentsRoutine());
        }
        else
        {
            scryingCameraRigObject = null;
            ScryingVCam = null;
            IsScryingDeployed = false;
        }
    }

    /// <summary>
    /// Finds the ScryingCameraRig in the newly loaded scene.
    /// </summary>
    private IEnumerator FindSceneComponentsRoutine()
    {
        // Using FindFirstObjectByType is the modern way to find objects.
        ScryingCameraRig rig = FindFirstObjectByType<ScryingCameraRig>(FindObjectsInactive.Include);
        if (rig != null)
        {
            scryingCameraRigObject = rig.gameObject;
            ScryingVCam = scryingCameraRigObject.GetComponentInChildren<CinemachineCamera>();
            scryingCameraRigObject.SetActive(false); // Ensure it starts disabled.
            Debug.Log("ScryingSystem successfully linked to ScryingCameraRig.");
            yield return new WaitUntil(() => GameManager.Instance.Player != null);

            if (ScryingVCam != null)
            {
                ScryingVCam.Follow = GameManager.Instance.Player.transform;
                Debug.Log("ScryingSystem: Player assigned as VCam Follow target.");
            }
        }
        else
        {
            Debug.LogError("ScryingSystem could not find the ScryingCameraRig in the scene! Ensure the rig exists and has the ScryingCameraRig component.");
        }
      
    }

    // This is called by ScryingEffectSO after the cast animation.
    public void DeployScryingEye()
    {
        if (IsScryingDeployed || scryingCameraRigObject == null) return;

        // The ONLY action needed: turn the independent rig on. No more interference.
        scryingCameraRigObject.SetActive(true);

        IsScryingDeployed = true;
        HUDManager.Instance.ShowMinimap(scryingRenderTexture);
        Debug.Log("Independent Scrying Camera Rig has been activated.");
    }

    public void DisableScryingEye()
    {
        if (!IsScryingDeployed || scryingCameraRigObject == null) return;

        // The ONLY action needed: turn the rig off.
        scryingCameraRigObject.SetActive(false);

        IsScryingDeployed = false;
        HUDManager.Instance.HideMinimap();
        Debug.Log("Independent Scrying Camera Rig has been deactivated.");
    }
}