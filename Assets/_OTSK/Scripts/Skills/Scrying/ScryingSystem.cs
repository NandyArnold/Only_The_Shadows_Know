using UnityEngine;

public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }

    private CameraController _cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void ActivateScrying(GameObject player)
    {
        if (_cameraController == null)
        {
            _cameraController = player.GetComponent<CameraController>();
        }

        Debug.Log("Activating Scrying...");
        _cameraController.ActivateScryingCamera();

        // TODO: Gather map/enemy data and show the Scrying UI.
    }

    public void DeactivateScrying()
    {
        if (_cameraController == null) return;

        Debug.Log("Deactivating Scrying...");
        _cameraController.DeactivateScryingCamera();
    }
}