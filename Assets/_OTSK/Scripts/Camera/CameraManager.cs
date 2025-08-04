// CameraManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine; // Or using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private List<CameraData> cameras;
    private Dictionary<CameraType, CinemachineCamera> _cameraDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Populate the dictionary for fast lookups
        _cameraDictionary = cameras.ToDictionary(x => x.type, x => x.camera);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCameraManager(this);
        }
    }

    public CinemachineCamera GetCamera(CameraType type)
    {
        _cameraDictionary.TryGetValue(type, out CinemachineCamera cam);
        return cam;
    }

    public void ConnectToPlayer(PlayerController player)
    {
        if (player.TryGetComponent<CameraController>(out var controller))
        {
            // Tell the CameraController on the player to initialize itself with this manager.
            controller.Initialize(this);
        }
    }
}