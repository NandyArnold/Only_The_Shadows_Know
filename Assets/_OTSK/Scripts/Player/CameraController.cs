// CameraController.cs

using UnityEngine;
using Unity.Cinemachine;// Required for Cinemachine

/// <summary>
/// Manages camera rotation based on player input.
/// Works with a Cinemachine Virtual Camera to provide a smooth third-person view.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The PlayerInputHandler to get look input from.")]
    private PlayerInputHandler playerInputHandler;
    [Tooltip("The empty GameObject the camera will follow and look at. This object will be rotated.")]
    [SerializeField] private Transform cameraTarget;

    [Header("Component References")] // NEW
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Cinemachine Cameras")] // UPDATED
    [SerializeField] private CinemachineCamera shoulderCamera;
    [SerializeField] private CinemachineCamera zoomCamera;


    [Header("Settings")]
    [Tooltip("Sensitivity of the mouse look.")]
    [SerializeField] private float lookSensitivity = 1.0f;
    [Tooltip("The highest angle (in degrees) the camera can look up.")]
    [SerializeField] private float topClamp = 70.0f;
    [Tooltip("The lowest angle (in degrees) the camera can look down.")]
    [SerializeField] private float bottomClamp = -30.0f;



    // Internal variables for rotation calculation
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector2 _lookInput;

    private void Awake()
    {
        if (playerInputHandler == null)
            playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();
    }

    void Start() // Use Start() to ensure the Player's Awake() has run first.
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            // Ask the GameManager for the player, then get the InputHandler component from it.
            playerInputHandler = GameManager.Instance.Player.GetComponent<PlayerInputHandler>();
            if (playerInputHandler != null)
            {
                // Now that we have the reference, subscribe to the event.
                playerInputHandler.OnLookInput += SetLookInput;
            }
        }
        else
        {
            Debug.LogError("CameraController could not find Player reference from GameManager!");
        }
    }
    private void OnEnable()
    {
        if (playerInputHandler != null)
            playerInputHandler.OnLookInput += SetLookInput;
        if (playerCombat != null)
            playerCombat.OnAimStateChanged += HandleAimStateChanged;
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
            playerInputHandler.OnLookInput -= SetLookInput;
        if (playerCombat != null)
            playerCombat.OnAimStateChanged -= HandleAimStateChanged;
    }

    private void LateUpdate()
    {
        // LateUpdate is used for cameras to ensure all player movement has been processed first.
        ApplyCameraRotation();
    }

    private void SetLookInput(Vector2 input)
    {
        _lookInput = input;
    }

    private void ApplyCameraRotation()
    {
        if (_lookInput.sqrMagnitude >= 0.01f) // Check for significant input
        {
            // Note: We multiply by Time.deltaTime to make it frame-rate independent.
            _cinemachineTargetYaw += _lookInput.x * lookSensitivity * Time.deltaTime;
            _cinemachineTargetPitch -= _lookInput.y * lookSensitivity * Time.deltaTime;
        }

        // Clamp the pitch rotation to avoid flipping over
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, bottomClamp, topClamp);

        // Clamp the yaw to a 360-degree range
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

        // Apply the calculated rotations to the camera target transform
        if (cameraTarget != null)
        {
            cameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void HandleAimStateChanged(bool isAiming)
    {
        if (shoulderCamera == null || zoomCamera == null) return;

        // A higher priority camera becomes the active one.
        shoulderCamera.Priority = isAiming ? 5 : 10;
        zoomCamera.Priority = isAiming ? 15 : 5;
    }

}