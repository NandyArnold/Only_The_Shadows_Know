// CameraType.cs
using Unity.Cinemachine; // Or using Cinemachine;
using System;

public enum CameraType
{
    Shoulder,
    Zoom,
    Scrying,
    Targeting,
    BalorsVision
}

[Serializable]
public class CameraData
{
    public CameraType type;
    public CinemachineCamera camera;
}