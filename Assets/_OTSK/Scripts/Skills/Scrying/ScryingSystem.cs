// ScryingSystem.cs - Final Definitive Version

using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }
    public RenderTexture ScryingRenderTexture => scryingRenderTexture;
    [Header("Configuration")]
    [SerializeField] private RenderTexture scryingRenderTexture;

    [Header("Icon System Prefab")]
    [SerializeField] private GameObject scryingIconCanvasPrefab; // Assign your canvas prefab
    [SerializeField] private TacticalIconAtlasSO iconAtlas;
    private Dictionary<Enemy, ScryingIcon> activeIcons = new Dictionary<Enemy, ScryingIcon>();
    private Camera scryingRenderCamera;
    private List<ScryingIconController> activeIconControllers = new List<ScryingIconController>();

    private Canvas scryingIconCanvasInstance;
    private RectTransform minimapIconContainer;
    private RectTransform fullMapIconContainer;
    private List<Image> iconPool = new List<Image>();

    private TacticalMapController tacticalMapController;
    private GameObject scryingCameraRigObject;
    public Camera ScryingRenderCamera { get; private set; }
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

    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        activeIconControllers.Clear();
        scryingCameraRigObject = null;
        IsScryingDeployed = false;
            ScryingVCam = null;

        if (sceneData.sceneType == SceneType.Gameplay)
        {
            StartCoroutine(FindSceneComponentsRoutine());
        }
        //else
        //{
        //    scryingCameraRigObject = null;
        //    IsScryingDeployed = false;
           
        //}
    }

    private IEnumerator FindSceneComponentsRoutine()
    {
        // Using FindFirstObjectByType is the modern way to find objects.
        ScryingCameraRig rig = FindFirstObjectByType<ScryingCameraRig>(FindObjectsInactive.Include);
        if (rig != null)
        {
            scryingCameraRigObject = rig.gameObject;
            ScryingVCam = scryingCameraRigObject.GetComponentInChildren<CinemachineCamera>();
            ScryingRenderCamera = scryingCameraRigObject.GetComponentInChildren<Camera>();
            scryingCameraRigObject.SetActive(false);
            //Debug.Log("ScryingSystem successfully linked to ScryingCameraRig.");

            if (scryingIconCanvasInstance != null)
            {
                scryingIconCanvasInstance.worldCamera = ScryingRenderCamera;
            }

            yield return new WaitUntil(() => GameManager.Instance.Player != null);

            if (ScryingVCam != null)
            {
                var playerTransform = GameManager.Instance.Player.transform;
                ScryingVCam.Follow = playerTransform;
                ScryingVCam.LookAt = playerTransform;

                Debug.Log("ScryingSystem: Player assigned as VCam Follow target.");
            }
        }
        else
        {
            Debug.LogError("ScryingSystem could not find the ScryingCameraRig in the scene! Ensure the rig exists and has the ScryingCameraRig component.");
        }
        tacticalMapController = FindFirstObjectByType<TacticalMapController>(FindObjectsInactive.Include);
        //InitializeIconPool();


    }

    // This is called by ScryingEffectSO after the cast animation.
    public void DeployScryingEye()
    {
        if (IsScryingDeployed || scryingCameraRigObject == null) return;

        // The ONLY action needed: turn the independent rig on. No more interference.
        scryingCameraRigObject.SetActive(true);

        if (ScryingRenderCamera != null && tacticalMapController != null)
        {
            ScryingRenderCamera.orthographicSize = tacticalMapController.MinimapDefaultZoom;
        }

        IsScryingDeployed = true;
        HUDManager.Instance.ShowMinimap(scryingRenderTexture);
        foreach (var controller in activeIconControllers)
        {
            controller.ShowIcon();
        }
        //Debug.Log("Independent Scrying Camera Rig has been activated.");
    }

    public void DisableScryingEye()
    {
        if (!IsScryingDeployed || scryingCameraRigObject == null) return;
       

        // The ONLY action needed: turn the rig off.
        scryingCameraRigObject.SetActive(false);

        IsScryingDeployed = false;
        HUDManager.Instance.HideMinimap();
        foreach (var controller in activeIconControllers)
        {
            if (controller != null) controller.HideIcon();
        }
        Debug.Log("Independent Scrying Camera Rig has been deactivated.");
    }

    public void RegisterIconController(ScryingIconController controller)
    {
        if (!activeIconControllers.Contains(controller))
        {
            activeIconControllers.Add(controller);
            // If scrying is already active when this icon spawns, tell it to show immediately
            if (IsScryingDeployed)
            {
                controller.ShowIcon();
            }
        }
    }

    public void UnregisterIconController(ScryingIconController controller)
    {
        if (activeIconControllers.Contains(controller))
        {
            activeIconControllers.Remove(controller);
        }
    }

    private void LateUpdate()
    {
        // If scrying is active, tell all icons to face the camera (billboarding)
        if (IsScryingDeployed && ScryingRenderCamera != null)
        {
            Quaternion cameraRotation = ScryingRenderCamera.transform.rotation;
            foreach (var controller in activeIconControllers)
            {
                if (controller != null) controller.UpdateRotation(cameraRotation);
            }
        }
    }
}