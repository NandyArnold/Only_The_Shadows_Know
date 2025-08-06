// In ScryingSystem.cs
using UnityEngine;

public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }
    public bool IsScryingActive { get; private set; } // NEW

    private CameraController _cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        
        
    }

    // NEW: A single method to toggle the scrying state.
    public void ToggleScrying(GameObject player)
    {
        if (IsScryingActive)
        {
            DeactivateScrying();
        }
        else
        {
            ActivateScrying(player);
        }
    }

    public void ActivateScrying(GameObject player)
    {
        if (_cameraController == null)
            _cameraController = player.GetComponent<CameraController>();

        _cameraController.ActivateScryingCamera();
        IsScryingActive = true; // Set state
        Debug.Log("Activating Scrying...");
    }

    public void DeactivateScrying()
    {
        if (_cameraController == null) return;

        _cameraController.DeactivateScryingCamera();
        IsScryingActive = false; // Set state
        Debug.Log("Deactivating Scrying...");
    }
}