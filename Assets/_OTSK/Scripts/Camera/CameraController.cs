// CameraController.cs - REFACTORED for Focus State
using DG.Tweening;
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    //CineMachine Cameras
    private CinemachineCamera shoulderCamera;
    private CinemachineCamera zoomCamera;
    private CinemachineCamera scryingCamera;
    private CinemachineCamera targetingCamera;
    private CinemachineCamera balorsVisionCamera;
    private CinemachineCamera focusedShotCamera;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerCombat playerCombat;


    [Tooltip("The empty object the camera follows. This is what we rotate.")]
    [SerializeField] private Transform cameraTarget;

    [Header("Settings")]
    [SerializeField] private float lookSensitivity = 1.0f;
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;

    [Header("Targeting Settings")] 
    [SerializeField] private float targetingPitchMin = 30f;
    [SerializeField] private float targetingPitchMax = 80f;

    [Header("Balor's Vision FX Settings")] // NEW
    [SerializeField] private float fovPulseAmount = 180f;
    [SerializeField] private float fovPulseHangTime = 0.1f;
    [SerializeField] private float fovPulseDuration = 0.5f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector2 _lookInput;

    private float _pitchAdjustInput;

    private CinemachineBrain _cinemachineBrain;



    

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnLookInput += SetLookInput;
            playerInputHandler.OnAdjustPitchInput += SetPitchAdjustInput;
        }
        // UPDATED: Subscribe to the focus event, not the aim event.
        if (playerCombat != null)
            playerCombat.OnFocusStateChanged += HandleFocusStateChanged;
           
    }

    public void Initialize(CameraManager manager)
    {
        Debug.Log("CameraController Initializing...");

        // Get camera references from the manager
        shoulderCamera = manager.GetCamera(CameraType.Shoulder);
        zoomCamera = manager.GetCamera(CameraType.Zoom);
        scryingCamera = manager.GetCamera(CameraType.Scrying);
        targetingCamera = manager.GetCamera(CameraType.Targeting);
        balorsVisionCamera = manager.GetCamera(CameraType.BalorsVision);
        focusedShotCamera = manager.GetCamera(CameraType.FocusedShot);

        // Get local components
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerCombat = GetComponent<PlayerCombat>();
        _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        // Find the camera target pivot
        if (cameraTarget == null)
        {
            cameraTarget = transform.Find("PlayerCameraTarget");
        }
        if (shoulderCamera != null && cameraTarget != null)
        {
            shoulderCamera.Follow = cameraTarget;
            shoulderCamera.LookAt = cameraTarget;
        }
        if (zoomCamera != null && cameraTarget != null)
        {
            zoomCamera.Follow = cameraTarget;
            zoomCamera.LookAt = cameraTarget;
        }
        if (targetingCamera != null && cameraTarget != null)
        {
            targetingCamera.Follow = cameraTarget;
            targetingCamera.LookAt = cameraTarget;
        }
        if (scryingCamera != null)
        {
            // Scrying camera should be static, so it does not follow the player.
            scryingCamera.Follow = null;
            scryingCamera.LookAt = null;
        }
        if (balorsVisionCamera != null && cameraTarget != null)
        { 
            balorsVisionCamera.Follow = cameraTarget;
            balorsVisionCamera.LookAt = cameraTarget;
        }
        if (focusedShotCamera != null && cameraTarget != null) 
        { 
            focusedShotCamera.Follow = cameraTarget; focusedShotCamera.LookAt = cameraTarget; 
        }

        // Subscribe to events now that we know everything exists
        if (playerInputHandler != null)
        {
            playerInputHandler.OnLookInput += SetLookInput;
            playerInputHandler.OnAdjustPitchInput += SetPitchAdjustInput;
        }
        if (playerCombat != null)
        {
            playerCombat.OnFocusStateChanged += HandleFocusStateChanged;
          
        }

        SetDefaultCameraState();
    }

    public void SwitchToCamera(CameraType type,float duration = 0.25f)
    {
        // Reset all camera priorities to a low value first
        shoulderCamera.Priority = 10;
        zoomCamera.Priority = 10;
        scryingCamera.Priority = 10;
        targetingCamera.Priority = 10;
        balorsVisionCamera.Priority = 10;
        focusedShotCamera.Priority = 10;

        // Then, give the target camera the highest priority
        var targetCam = CameraManager.Instance.GetCamera(type);
        if (targetCam == null) return;

        // --- SPECIAL TRANSITION FOR BALOR'S VISION ---
        if (type == CameraType.BalorsVision)
        {
            targetCam.Priority = 20;
            StartCoroutine(BalorsVisionFXRoutine(targetCam));
        }
        else
        {
            // --- Standard Instant Switch for all other cameras ---
            shoulderCamera.Priority = 10;
            zoomCamera.Priority = 10;
            scryingCamera.Priority = 10;
            targetingCamera.Priority = 10;
            balorsVisionCamera.Priority = 10;

            targetCam.Priority = 20;
        }
    }

    private IEnumerator BalorsVisionFXRoutine(CinemachineCamera vcam)
    {
        // We use a small delay to let the camera blend finish first.
        yield return new WaitForSeconds(0.1f);

        float originalFov = vcam.Lens.FieldOfView;

        // Use a DOTween Sequence for the multi-part animation.
        DOTween.Sequence()
            .Append(DOTween.To(() => vcam.Lens.FieldOfView, x => vcam.Lens.FieldOfView = x, fovPulseAmount, fovPulseDuration / 2).SetEase(Ease.OutQuad))
            .AppendInterval(fovPulseHangTime)
            .Append(DOTween.To(() => vcam.Lens.FieldOfView, x => vcam.Lens.FieldOfView = x, originalFov, fovPulseDuration / 2).SetEase(Ease.InQuad));
    }

    private void SetDefaultCameraState()
    {
        if (shoulderCamera == null) return;

        // Give the shoulder camera the highest priority to make it active.
        shoulderCamera.Priority = 20;
        zoomCamera.Priority = 10;
        targetingCamera.Priority = 10;
        scryingCamera.Priority = 10;
    }
    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnLookInput -= SetLookInput;
            playerInputHandler.OnAdjustPitchInput -= SetPitchAdjustInput;
        }
        //  Unsubscribe from the focus event.
        if (playerCombat != null)
            playerCombat.OnFocusStateChanged -= HandleFocusStateChanged;
           
    }

    
    private void LateUpdate()
    {
        ApplyCameraRotation();
    }

    // UPDATED: This method now handles the zoom by switching camera priorities based on focus.
    private void HandleFocusStateChanged(bool isFocused)
    {
        if (shoulderCamera == null || zoomCamera == null) return;

        // This logic is now simpler: just switch between shoulder and zoom
        SwitchToCamera(isFocused ? CameraType.Zoom : CameraType.Shoulder);
    }

    private void HandleFocusedShotFired()
    {
        StartCoroutine(FocusedShotRecoilRoutine());
    }

    private IEnumerator FocusedShotRecoilRoutine()
    {
        // Switch to the recoil camera
        SwitchToCamera(CameraType.FocusedShot);

        // Wait for a very short moment. This is how long the recoil "kicks" for.
        yield return new WaitForSeconds(0.2f);

        // After the wait, switch back to the standard zoom camera.
        SwitchToCamera(CameraType.Zoom);
    }
    private void SetLookInput(Vector2 input)
    {
        _lookInput = input;
    }

    private void SetPitchAdjustInput(float pitchInput)
    {
        _pitchAdjustInput = pitchInput;
    }
    private void ApplyCameraRotation()
    {
        // THIS IS THE FIX: We get the currently active camera from the brain
        // and compare it to our targetingCamera reference.
        if (_cinemachineBrain != null && (Object)_cinemachineBrain.ActiveVirtualCamera == targetingCamera)
        {
            // If in targeting mode, only adjust pitch with the new input
            _cinemachineTargetPitch -= _pitchAdjustInput * lookSensitivity * Time.deltaTime;
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, targetingPitchMin, targetingPitchMax);
        }
        else
        {
            // Otherwise, use the standard gameplay look controls
            if (_lookInput.sqrMagnitude >= 0.01f)
            {
                _cinemachineTargetYaw += _lookInput.x * lookSensitivity * Time.deltaTime;
                _cinemachineTargetPitch -= _lookInput.y * lookSensitivity * Time.deltaTime;
            }
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, bottomClamp, topClamp);
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        if (cameraTarget != null)
        {
            cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }
    }
       


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    public void ActivateScryingCamera()
    {
        scryingCamera.Priority = 20; // Give it the highest priority
    }

    public void DeactivateScryingCamera()
    {
        scryingCamera.Priority = 5; // Return priority to normal
        shoulderCamera.Priority = 10;
        zoomCamera.Priority = 5;
    }

    public void ActivateTargetingCamera()
    {
        targetingCamera.Priority = 20; // Give it the highest priority
    }

    public void DeactivateTargetingCamera()
    {
        targetingCamera.Priority = 5; // Return priority to normal
        shoulderCamera.Priority = 10;
        zoomCamera.Priority = 5;
    }

 
    



}