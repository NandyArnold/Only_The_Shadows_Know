// CameraController.cs - REFACTORED for Focus State

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

    [Header("Recoil Settings")]
    [SerializeField] private float recoilShakeAmplitude = 2f;
    [SerializeField] private float recoilShakeDuration = 0.2f;
    [SerializeField] private float recoilFovKick = 5f;
    [SerializeField] private float recoilKickDuration = 0.1f;
    [SerializeField] private float recoilReturnDuration = 0.25f;


    //[Header("Aiming Settings")]
    //[SerializeField] private float normalAimFov = 40f;
    //[SerializeField] private float focusedAimFov = 25f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector2 _lookInput;

    private float _pitchAdjustInput;

    private CinemachineBrain _cinemachineBrain;

    private Coroutine _recoilCoroutine;

    

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
            playerCombat.OnFocusedShotFired += HandleFocusedShotFired;
    }

    public void Initialize(CameraManager manager)
    {
        Debug.Log("CameraController Initializing...");

        // Get camera references from the manager
        shoulderCamera = manager.GetCamera(CameraType.Shoulder);
        zoomCamera = manager.GetCamera(CameraType.Zoom);
        scryingCamera = manager.GetCamera(CameraType.Scrying);
        targetingCamera = manager.GetCamera(CameraType.Targeting);

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

        // Subscribe to events now that we know everything exists
        if (playerInputHandler != null)
        {
            playerInputHandler.OnLookInput += SetLookInput;
            playerInputHandler.OnAdjustPitchInput += SetPitchAdjustInput;
        }
        if (playerCombat != null)
        {
            playerCombat.OnFocusStateChanged += HandleFocusStateChanged;
            playerCombat.OnFocusedShotFired += HandleFocusedShotFired;
        }

        SetDefaultCameraState();
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
            playerCombat.OnFocusedShotFired -= HandleFocusedShotFired;
    }

    
    private void LateUpdate()
    {
        ApplyCameraRotation();
    }

    // UPDATED: This method now handles the zoom by switching camera priorities based on focus.
    private void HandleFocusStateChanged(bool isFocused)
    {
        if (shoulderCamera == null || zoomCamera == null) return;

        // When focused, zoom camera is higher priority. When not, shoulder camera is.
        shoulderCamera.Priority = isFocused ? 5 : 10;
        zoomCamera.Priority = isFocused ? 15 : 5;
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

    private void HandleFocusedShotFired()
    {
        if (_recoilCoroutine != null)
        {
            StopCoroutine(_recoilCoroutine);
        }
        // Start the new recoil and store a reference to it.
        _recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
       ICinemachineCamera liveCamera = _cinemachineBrain.ActiveVirtualCamera;
        if (liveCamera == null) yield break;

        var noise = zoomCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null) yield break;

        var vcam = liveCamera as CinemachineCamera;

        // 1. Store the camera's original FOV before we change anything.
        float originalFov = vcam.Lens.FieldOfView;
        float targetKickFov = originalFov + recoilFovKick;

        // --- KICK ---
        noise.AmplitudeGain = recoilShakeAmplitude;
        float elapsedTime = 0f;
        while (elapsedTime < recoilKickDuration)
        {
            float t = elapsedTime / recoilKickDuration;
            vcam.Lens.FieldOfView = Mathf.SmoothStep(originalFov, targetKickFov, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        vcam.Lens.FieldOfView += recoilFovKick;

        // --- RETURN ---
        yield return new WaitForSeconds(recoilShakeDuration);

        noise.AmplitudeGain = 0f;

        // --- SMOOTHLY RETURN TO ORIGINAL FOV ---
        elapsedTime = 0f;
        float startFov = vcam.Lens.FieldOfView;
        // 2. Set the target FOV for the return trip to be the original value.
        float targetFov = originalFov;

        while (elapsedTime < 0.2f) // 0.2s to return to normal
        {   
            float t = elapsedTime / recoilReturnDuration; // Normalize time
            vcam.Lens.FieldOfView = Mathf.SmoothStep(startFov, originalFov, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        vcam.Lens.FieldOfView = originalFov;
    }

}