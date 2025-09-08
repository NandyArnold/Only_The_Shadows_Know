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
 
    private Camera scryingRenderCamera;

    [Header("Icon System Settings")]
    [Tooltip("A base size for the icons in world space.")]
    [SerializeField] private float iconBaseSize = 1f;
    [Tooltip("How much the icon size is multiplied by the camera's zoom level.")]
    [SerializeField] private float iconSizeScalar = 0.1f;

    [Header("Icon Decluttering")]
    [SerializeField] private bool enableDecluttering = true;
    [Tooltip("How many icon-widths apart should icons be? 1.5 means 1.5x the icon's current width.")]
    [SerializeField] private float minIconDistance = 1.5f;
    [Tooltip("How much camera zoom affects the decluttering distance. Small values (e.g., 0.1) work best.")]
    [SerializeField] private float declutterZoomScalar = 0.1f;
    [SerializeField] private float repulsionStrength = 0.5f;

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
            SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent issues.
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
        }
    }

    private void HandleSceneLoadCompleted(SceneDataSO sceneData)
    {
        // 1. Perform all the necessary resets.
        activeIconControllers.Clear();
        scryingCameraRigObject = null;
        IsScryingDeployed = false;
        ScryingVCam = null;

        // 2. Check the scene type, just like before.
        if (sceneData.sceneType == SceneType.Gameplay)
        {
            // 3. Start the coroutine to find the camera rig and player.
            StartCoroutine(FindSceneComponentsRoutine());
        }
    }

    private IEnumerator FindSceneComponentsRoutine()
    {
        // Find the rig and its components as before.
        ScryingCameraRig rig = FindFirstObjectByType<ScryingCameraRig>(FindObjectsInactive.Include);
        if (rig != null)
        {
            scryingCameraRigObject = rig.gameObject;
            ScryingVCam = scryingCameraRigObject.GetComponentInChildren<CinemachineCamera>();
            ScryingRenderCamera = scryingCameraRigObject.GetComponentInChildren<Camera>();
            scryingCameraRigObject.SetActive(false);

            // Your line for the canvas camera is preserved.
            if (scryingIconCanvasInstance != null)
            {
                scryingIconCanvasInstance.worldCamera = ScryingRenderCamera;
            }
        }
        else
        {
            Debug.LogError("ScryingSystem could not find the ScryingCameraRig!");
        }

        // --- NEW LOGIC TO HANDLE BOTH CASES ---
        // If a save game is being loaded, we stop this coroutine immediately.
        // The SaveLoadManager will be responsible for the final setup steps.
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.IsLoading)
        {
            yield break; // This exits the coroutine.
        }

        // --- NORMAL SCENE LOAD PATH ---
        // If we are NOT loading a save, this coroutine finishes the setup itself.
        yield return new WaitUntil(() => GameManager.Instance.Player != null);

        ConnectToPlayer();
        RegisterAllIconsInScene();

        tacticalMapController = FindFirstObjectByType<TacticalMapController>(FindObjectsInactive.Include);
    }

    public void ConnectToPlayer()
    {
        if (ScryingVCam != null && GameManager.Instance.Player != null)
        {
            var playerTransform = GameManager.Instance.Player.transform;
            ScryingVCam.Follow = playerTransform;
            ScryingVCam.LookAt = playerTransform;
            Debug.Log("ScryingSystem: Camera connection to player established by external call.");
        }
        else
        {
            Debug.LogWarning("ScryingSystem: ConnectToPlayer was called, but VCam or Player was null.");
        }
    }

    public void RegisterAllIconsInScene()
    {
        activeIconControllers.Clear();
      
        var allControllersInScene = FindObjectsByType<ScryingIconController>(FindObjectsSortMode.None);
        foreach (var controller in allControllersInScene)
        {
            controller.CreateIcon(scryingIconCanvasPrefab, iconAtlas);
            if (!activeIconControllers.Contains(controller))
            {
                activeIconControllers.Add(controller);
            }
            if (controller.IsOwnerDead)
            {
                ReportDeath(controller);
            }
        }
        Debug.Log($"ScryingSystem: Registered {activeIconControllers.Count} icons.");
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
        var controllers = FindObjectsByType<ScryingIconController>(FindObjectsSortMode.None);
        foreach (var controller in controllers)
        {
            controller.ShowIcon();
        }
        //Debug.Log("Independent Scrying Camera Rig has been activated.");
    }

    public void DisableScryingEye()
    {
        if (!IsScryingDeployed || scryingCameraRigObject == null) return;
       

        // The ONLY action needed: turn the rig off.
        HUDManager.Instance.HideMinimap();
        foreach (var controller in activeIconControllers)
        {
            if (controller != null) controller.HideIcon();
        }
        scryingCameraRigObject.SetActive(false);

        IsScryingDeployed = false;
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
        if (!IsScryingDeployed || ScryingRenderCamera == null) return;

        var activeControllers = activeIconControllers;
        Transform cameraTransform = ScryingRenderCamera.transform;

        float currentZoom = ScryingRenderCamera.orthographicSize;
        float desiredImageScale = iconBaseSize * currentZoom * iconSizeScalar;
        Vector3 scaleVector = Vector3.one * desiredImageScale;

        List<Transform> activeIconTransforms = new List<Transform>();

        foreach (var controller in activeControllers)
        {
            if (controller == null || controller.IconInstance == null) continue;

            // --- REVISED VISIBILITY LOGIC ---
            bool shouldBeVisible;

            // An icon should be visible if it IS an objective...
            if (controller.IsObjective)
            {
                shouldBeVisible = true;
            }
            else // ...OR if it is currently in the camera's view.
            {
                Vector3 viewportPoint = ScryingRenderCamera.WorldToViewportPoint(controller.TargetTransform.position);
                shouldBeVisible = viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1;
            }

            // Apply the visibility state.
            controller.IconInstance.SetActive(shouldBeVisible);

            // If the icon is not visible, skip all further processing for it.
            if (!shouldBeVisible)
            {
                continue;
            }

            // --- ALL VISIBLE ICONS (OBJECTIVES & IN-VIEW) ARE PROCESSED BELOW THIS LINE ---
            Transform iconTransform = controller.IconInstance.transform;
            Transform ownerTransform = controller.TargetTransform;

            // 1. Update Position
            iconTransform.position = new Vector3(ownerTransform.position.x, iconTransform.position.y, ownerTransform.position.z);

         
            iconTransform.rotation = cameraTransform.rotation;
            // 2. Set Sort Order
            if (controller.IsObjective)
            {
                controller.SetSortOrder(1); // Objectives on top
            }
            else
            {
                controller.SetSortOrder(0); // Everything else below
            }


            // 3. Update Scale
            if (controller.IconImageRectTransform != null)
            {
                controller.IconImageRectTransform.localScale = scaleVector;
            }

            // 4. Add to list for decluttering
            activeIconTransforms.Add(iconTransform);
        }

        if (enableDecluttering)
        {
            float effectiveDeclutterDistance = minIconDistance + (currentZoom * declutterZoomScalar);
            HandleIconOverlapping(activeIconTransforms, effectiveDeclutterDistance);
        }
    }


    private void HandleIconOverlapping(List<Transform> iconTransforms, float effectiveMinDistance)
    {
        for (int i = 0; i < iconTransforms.Count; i++)
        {
            for (int j = i + 1; j < iconTransforms.Count; j++)
            {
                Transform iconA = iconTransforms[i];
                Transform iconB = iconTransforms[j];

                float distance = Vector3.Distance(iconA.position, iconB.position);

                // 3. Use the new effectiveMinDistance instead of the fixed variable.
                if (distance < effectiveMinDistance)
                {
                    Vector3 repulsionDir = (iconA.position - iconB.position).normalized;
                    // Use the effective distance in the push calculation as well.
                    float pushAmount = (effectiveMinDistance - distance) * repulsionStrength;

                    iconA.position += repulsionDir * pushAmount;
                    iconB.position -= repulsionDir * pushAmount;
                }
            }
        }
    }

    public void ReportDeath(ScryingIconController deceasedController)
    {
        if (deceasedController == null || deceasedController.IconInstance == null || iconAtlas.corpseIcon == null) return;

        Image iconImage = deceasedController.IconInstance.GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            // Use this system's atlas to change the sprite
            iconImage.sprite = iconAtlas.corpseIcon;
            //iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // A faded gray
        }

        // Set the sort order to appear underneath living icons
        deceasedController.SetSortOrder(-1);
    }

    public void ResetStateForLoad()
    {
        Debug.Log("ScryingSystem: Resetting state for game load.");
        if (IsScryingDeployed)
        {
            // Use your existing method to ensure a clean shutdown
            DisableScryingEye();
        }
        // Also clear the list, as it will be repopulated later.
        activeIconControllers.Clear();
    }

}