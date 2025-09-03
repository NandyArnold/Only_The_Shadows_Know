// TacticalMapController.cs
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TacticalMapController : MonoBehaviour
{
   
   //"The collider that defines the total map area.
    private Collider mapBoundary;

    [Header("Settings")]
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float zoomSpeed = 20f;

    [Header("Zoom Clamping")]
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 180f;

    [Header("Default Zoom Levels")]
    [SerializeField] private float minimapDefaultZoom = 20f;
    [SerializeField] private float tacticalMapDefaultZoom = 50f;

    private CinemachineCamera scryingVCam;
    private CinemachineCameraOffset cameraOffset;
    private Camera scryingRenderCamera;



    private PlayerInputActions playerInputActions;
    private Transform originalFollowTarget;
    private Transform originalLookAtTarget;


    public float MinimapDefaultZoom => minimapDefaultZoom;

    private void OnEnable()
    {
        MapBoundary boundary = FindFirstObjectByType<MapBoundary>();
        if (boundary != null)
        {
            mapBoundary = boundary.GetComponent<Collider>();
        }
        if (mapBoundary == null)
        {
            Debug.LogWarning("TacticalMapController could not find a MapBoundary in the scene. Panning will not be clamped.");
        }

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            playerInputActions = GameManager.Instance.Player.GetComponent<PlayerInputHandler>()._inputActions;
        }

        // Get the VCam reference from the ScryingSystem (Just-in-Time)
        if (ScryingSystem.Instance != null)
        {
            scryingVCam = ScryingSystem.Instance.ScryingVCam;
            scryingRenderCamera = ScryingSystem.Instance.ScryingRenderCamera;

            if (scryingVCam != null)
            {
                cameraOffset = scryingVCam.GetComponent<CinemachineCameraOffset>();

                // *** THE LOGIC IS HERE ***
                // Store BOTH original targets
                originalFollowTarget = scryingVCam.Follow;
                originalLookAtTarget = scryingVCam.LookAt;

                // Clear targets for free-panning
                scryingVCam.Follow = null;
                scryingVCam.LookAt = null;
            }
        }

        if (cameraOffset == null || scryingRenderCamera == null)
        {
            Debug.LogError("TacticalMapController could not get required Camera components!", this);
            this.enabled = false;
        }
        if (scryingRenderCamera != null)
        {
            scryingRenderCamera.orthographicSize = tacticalMapDefaultZoom;
        }

    }

    private void OnDisable()
    {
        if (scryingVCam != null && originalFollowTarget != null)
        {
            scryingVCam.Follow = originalFollowTarget;
            scryingVCam.LookAt = originalLookAtTarget;
        }
        if (scryingRenderCamera != null)
        {
            scryingRenderCamera.orthographicSize = minimapDefaultZoom;
        }
        // Also reset any panning/zooming
        if (cameraOffset != null)
        {
            cameraOffset.Offset = Vector3.zero;
        }
    
    }
    private void Update()
    {
        // If our setup failed for any reason, do nothing.
        if (playerInputActions == null || cameraOffset == null) return;

        HandlePan();
        HandleZoom();
    }


      
    private void HandleZoom()
    {
        if (scryingRenderCamera == null) return;
        // This is the fix for the InvalidOperationException
        float scrollDelta = playerInputActions.UI.MapZoom.ReadValue<float>();

        if (Mathf.Abs(scrollDelta) > 0.1f) // A small threshold
        {
            float newSize = scryingRenderCamera.orthographicSize - scrollDelta * zoomSpeed * Time.deltaTime;
            scryingRenderCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

            ClampCameraPosition();
        }
    }

    private void HandlePan()
    {
        if (cameraOffset == null) return;
        bool isPanModifierHeld = playerInputActions.UI.MapPanModifier.IsPressed();

        if (isPanModifierHeld)
        {
            Vector2 panDelta = playerInputActions.UI.MapPan.ReadValue<Vector2>();
            cameraOffset.Offset.x -= panDelta.x * panSpeed * Time.deltaTime;
            cameraOffset.Offset.y -= panDelta.y * panSpeed * Time.deltaTime;

            ClampCameraPosition();
        }
    }

    private void ClampCameraPosition()
    {
        if (scryingVCam == null || mapBoundary == null) return;

        // 1. Get the VCam's base position (before the pan offset).
        Vector3 basePosition = scryingVCam.transform.position;

        // 2. Calculate our desired final position in WORLD space by converting the LOCAL offset to a world direction.
        Vector3 desiredPosition = basePosition + scryingVCam.transform.TransformDirection(cameraOffset.Offset);

        // 3. Find the closest valid point within the boundary in WORLD space.
        Vector3 clampedWorldPosition = mapBoundary.ClosestPoint(desiredPosition);

        // 4. Calculate the corrected offset vector in WORLD space.
        Vector3 correctedWorldOffset = clampedWorldPosition - basePosition;

        // 5. Convert the corrected WORLD offset back into the VCam's LOCAL space and apply it.
        cameraOffset.Offset = scryingVCam.transform.InverseTransformDirection(correctedWorldOffset);
    }


}