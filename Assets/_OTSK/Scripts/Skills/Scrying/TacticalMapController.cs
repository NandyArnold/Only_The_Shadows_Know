// TacticalMapController.cs
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class TacticalMapController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private float panSpeed = 0.1f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomOffset = -180f;
    [SerializeField] private float maxZoomOffset = -10f;

    private CinemachineCamera scryingVCam;
    private CinemachineCameraOffset cameraOffset;
    private PlayerInputHandler playerInputHandler;
    private PlayerInputActions playerInputActions;
    private Transform originalFollowTarget;
    private bool isPanning = false;
    private bool isControlActive = false;


    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            playerInputActions = GameManager.Instance.Player.GetComponent<PlayerInputHandler>()._inputActions;
        }

        // Get the VCam reference from the ScryingSystem (Just-in-Time)
        if (scryingVCam == null && ScryingSystem.Instance != null)
        {
            scryingVCam = ScryingSystem.Instance.ScryingVCam;
            if (scryingVCam != null)
            {
                cameraOffset = scryingVCam.GetComponent<CinemachineCameraOffset>();
            }
        }

        if (cameraOffset == null)
        {
            Debug.LogError("TacticalMapController could not get CameraOffset component!");
            this.enabled = false; // Disable self if setup fails
            return;
        }

        // Store the original target and disable following
        originalFollowTarget = scryingVCam.Follow;
        scryingVCam.Follow = null;
        //// Subscribe to the SceneLoader to know when to find our references.
        //if (SceneLoader.Instance != null)
        //{
        //    SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoaded;
        //}
    }

    private void OnDisable()
    {
        if (scryingVCam != null && originalFollowTarget != null)
        {
            scryingVCam.Follow = originalFollowTarget;
        }

        // Also reset any panning/zooming
        if (cameraOffset != null)
        {
            cameraOffset.Offset = Vector3.zero;
        }
        // Always unsubscribe to prevent errors.
        //if (SceneLoader.Instance != null)
        //{
        //    SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        //}
        //UnsubscribeFromInput();
    }
    private void Update()
    {
        // If our setup failed for any reason, do nothing.
        if (playerInputActions == null || cameraOffset == null) return;

        // --- Polling Input directly from the UI Action Map ---

        // Handle Zooming
        float scrollDelta = playerInputActions.UI.MapZoom.ReadValue<Vector2>().y;
        if (Mathf.Abs(scrollDelta) > 0.1f) // A small threshold to avoid tiny inputs
        {
            cameraOffset.Offset.z += scrollDelta * zoomSpeed * Time.deltaTime;
            cameraOffset.Offset.z = Mathf.Clamp(cameraOffset.Offset.z, minZoomOffset, maxZoomOffset);
        }

        // Handle Panning
        bool isPanModifierHeld = playerInputActions.UI.MapPanModifier.IsPressed();
        if (isPanModifierHeld)
        {
            Vector2 panDelta = playerInputActions.UI.MapPan.ReadValue<Vector2>();
            cameraOffset.Offset.x -= panDelta.x * panSpeed * Time.deltaTime;
            cameraOffset.Offset.y -= panDelta.y * panSpeed * Time.deltaTime;
        }
    }

    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        // We only search for components in a gameplay scene.
        if (sceneData.sceneType == SceneType.Gameplay)
        {
            StartCoroutine(GetReferencesRoutine());
        }
        else
        {
            // If we're not in a gameplay scene, clear references.
            scryingVCam = null;
            UnsubscribeFromInput();
        }
    }

    private IEnumerator GetReferencesRoutine()
    {
        // Wait for ScryingSystem to be ready
        yield return new WaitUntil(() => ScryingSystem.Instance != null && ScryingSystem.Instance.ScryingVCam != null);

        // *** THE FIX: Get the VCam reference from the ScryingSystem ***
        scryingVCam = ScryingSystem.Instance.ScryingVCam;

        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Player != null);

        playerInputHandler = GameManager.Instance.Player.GetComponent<PlayerInputHandler>();
        SubscribeToInput();
    }

    // Called by UIManager when the map is opened
    public void TakeControl()
    {
        //if (scryingVCam == null && ScryingSystem.Instance != null)
        //{
        //    scryingVCam = ScryingSystem.Instance.ScryingVCam;
        //}
        //Debug.Log($"[TacticalMapController] TakeControl called. Is scryingVCam null? {scryingVCam == null}");
        if (scryingVCam == null) return;
        cameraOffset = scryingVCam.GetComponent<CinemachineCameraOffset>();
        if (cameraOffset == null)
        {
            Debug.LogError("TacticalMapController FAILED: The scryingVCam is missing the 'CinemachineCameraOffset' extension!");
            return;
        }

        // Store the original target and then disable following so we can pan freely.
        Debug.Log($"[TacticalMapController] Before change, Follow target is: {scryingVCam.Follow?.name ?? "NULL"}");
        originalFollowTarget = scryingVCam.Follow;
        scryingVCam.Follow = null;

        Debug.Log($"[TacticalMapController] After change, Follow target is: {scryingVCam.Follow?.name ?? "NULL"}");

        isControlActive = true;
        //playerInputHandler = GameManager.Instance.Player.GetComponent<PlayerInputHandler>();
        //SubscribeToInput();
        //this.enabled = true; // Ensures the component is active, though not strictly needed if always on.
    }

    // Called by UIManager when the map is closed
    public void ReleaseControl()
    {
        if (scryingVCam == null) return;

        // Restore the original follow target.
        scryingVCam.Follow = originalFollowTarget;
        //isControlActive = false;
        Debug.Log($"[TacticalMapController] Control released. Follow target restored to: {scryingVCam.Follow?.name ?? "NULL"}");
        originalFollowTarget = null;
        //UnsubscribeFromInput();
    }

    // --- Input Subscription ---
    private void SubscribeToInput()
    {
        if (playerInputHandler == null) return;
        playerInputHandler.OnMapZoomInput += HandleZoom;
        playerInputHandler.OnMapPanInput += HandlePan;
        playerInputHandler.OnMapPanModifierInput += HandlePanModifier;
    }

    private void UnsubscribeFromInput()
    {
        if (playerInputHandler == null) return;
        playerInputHandler.OnMapZoomInput -= HandleZoom;
        playerInputHandler.OnMapPanInput -= HandlePan;
        playerInputHandler.OnMapPanModifierInput -= HandlePanModifier;
    }

    // --- Input Handlers ---
    private void HandlePanModifier(bool isPressed)
    {
        if (!isControlActive) return;
        isPanning = isPressed;
    }

    private void HandlePan(Vector2 panDelta)
    {
        if (!isControlActive || !isPanning || cameraOffset == null) return;

        // Modify the X and Y properties of the offset to pan
        cameraOffset.Offset.x -= panDelta.x * panSpeed * Time.deltaTime;
        cameraOffset.Offset.y -= panDelta.y * panSpeed * Time.deltaTime;
    }

    private void HandleZoom(float scrollDelta)
    {
        if (!isControlActive || cameraOffset == null) return;

        // Modify the Z property of the offset to zoom in and out
        cameraOffset.Offset.z += scrollDelta * zoomSpeed * Time.deltaTime;
        cameraOffset.Offset.z = Mathf.Clamp(cameraOffset.Offset.z, minZoomOffset, maxZoomOffset);
    }


}