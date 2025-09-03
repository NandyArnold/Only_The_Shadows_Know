// StaticVCamDriver.cs
using UnityEngine;
using Unity.Cinemachine;


/// <summary>
/// A simple script that acts like a manual brain. It forces the Camera
/// on this GameObject to adopt the state of a single, specified Virtual Camera,
/// ignoring the global priority system entirely.
/// </summary>
[RequireComponent(typeof(Camera))]
public class StaticVCamDriver : MonoBehaviour
{
    [Tooltip("The single Virtual Camera this camera should follow.")]
    [SerializeField] private CinemachineCamera virtualCamera;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 currentPosition;
    private Quaternion currentRotation;


    private void OnEnable()
    {
        // When the camera is enabled, immediately snap to the VCam's current position
        // to prevent a visual jump from its last known position.
        if (virtualCamera != null)
        {
            // We get the state directly, without forcing an update
            transform.position = virtualCamera.State.GetFinalPosition();
            transform.rotation = virtualCamera.State.GetFinalOrientation();

            // Initialize our stored positions to this starting state.
            previousPosition = currentPosition = transform.position;
            previousRotation = currentRotation = transform.rotation;
        }
    }
    private void FixedUpdate()
    {
        if (virtualCamera == null) return;

        // Before we get the new state, the "current" state becomes the "previous" one.
        previousPosition = currentPosition;
        previousRotation = currentRotation;

        // Get the VCam's new target state for this physics tick.
        currentPosition = virtualCamera.State.GetFinalPosition();
        currentRotation = virtualCamera.State.GetFinalOrientation();
    }
    private void LateUpdate()
    {
        if (virtualCamera == null) return;

        // Calculate how far we are between the last physics update and the next one.
        // This value (alpha) will be between 0 and 1.
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        // Use Lerp and Slerp to find the perfectly smooth, in-between position and rotation.
        transform.position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);
        transform.rotation = Quaternion.Slerp(previousRotation, currentRotation, interpolationFactor);
    }
}