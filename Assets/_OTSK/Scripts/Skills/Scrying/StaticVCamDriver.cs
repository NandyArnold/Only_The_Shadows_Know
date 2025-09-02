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

    private void LateUpdate()
    {
        if (virtualCamera == null) return;

        // Manually update the VCam's state and apply it to this camera's transform.
        // This is what a CinemachineBrain does, but we are doing it for only one VCam.
        //virtualCamera.UpdateCameraState(Vector3.up, Time.deltaTime);
        transform.position = virtualCamera.State.GetFinalPosition();
        transform.rotation = virtualCamera.State.GetFinalOrientation();
    }
}