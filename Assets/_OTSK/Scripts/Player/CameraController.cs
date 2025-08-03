// CameraController.cs - REFACTORED for Focus State

using UnityEngine;

using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera shoulderCamera;
    [SerializeField] private CinemachineCamera zoomCamera;
    [SerializeField] private CinemachineCamera scryingCamera;
    [SerializeField] private CinemachineCamera targetingCamera;

    [Tooltip("The empty object the camera follows. This is what we rotate.")]
    [SerializeField] private Transform cameraTarget;

    [Header("Settings")]
    [SerializeField] private float lookSensitivity = 1.0f;
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector2 _lookInput;

    private void Awake()
    {
        if (playerInputHandler == null) playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerCombat == null) playerCombat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
            playerInputHandler.OnLookInput += SetLookInput;

        // UPDATED: Subscribe to the focus event, not the aim event.
        if (playerCombat != null)
            playerCombat.OnFocusStateChanged += HandleFocusStateChanged;
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
            playerInputHandler.OnLookInput -= SetLookInput;

        // UPDATED: Unsubscribe from the focus event.
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

        // When focused, zoom camera is higher priority. When not, shoulder camera is.
        shoulderCamera.Priority = isFocused ? 5 : 10;
        zoomCamera.Priority = isFocused ? 15 : 5;
    }

    private void SetLookInput(Vector2 input)
    {
        _lookInput = input;
    }

    private void ApplyCameraRotation()
    {
        // This logic is unchanged.
        if (_lookInput.sqrMagnitude >= 0.01f)
        {
            _cinemachineTargetYaw += _lookInput.x * lookSensitivity * Time.deltaTime;
            _cinemachineTargetPitch -= _lookInput.y * lookSensitivity * Time.deltaTime;
        }
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, bottomClamp, topClamp);
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

        if (cameraTarget != null)
        {
            cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }
        // In your full player setup, this rotation should be applied to a camera target pivot, not the player directly.
        // For now, this is fine, but the player's rotation should be handled by PlayerMovement.
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
    }

    public void ActivateTargetingCamera()
    {
        targetingCamera.Priority = 20; // Give it the highest priority
    }

    public void DeactivateTargetingCamera()
    {
        targetingCamera.Priority = 5; // Return priority to normal
    }
}