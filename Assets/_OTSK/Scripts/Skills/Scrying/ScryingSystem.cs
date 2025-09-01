// In ScryingSystem.cs
using UnityEngine;

public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }

    [Header("Component References")]
    [SerializeField] private GameObject scryingCameraObject; // Assign your Scrying Camera GameObject
    [SerializeField] private RenderTexture tacticalViewTexture;
    [SerializeField] private RenderTexture minimapTexture;

    public bool IsScryingDeployed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure the eye is off at the start
        if (scryingCameraObject != null) scryingCameraObject.SetActive(false);
        IsScryingDeployed = false;

    }

  

    public void DeployScryingEye(Vector3 position)
    {
        if (IsScryingDeployed) return;

        // Position and activate the camera
        scryingCameraObject.transform.position = position + new Vector3(0, 50, 0); // Example offset
        scryingCameraObject.SetActive(true);
        IsScryingDeployed = true;

        // Tell the HUD to show the minimap
        HUDManager.Instance.ShowMinimap(minimapTexture);
        Debug.Log("Scrying Eye has been deployed.");
    }

    public void DisableScryingEye()
    {
        if (!IsScryingDeployed) return;

        scryingCameraObject.SetActive(false);
        IsScryingDeployed = false;

        // Tell the HUD to hide the minimap and tactical view
        HUDManager.Instance.HideMinimap();
        // You'll need a reference to your UIManager or the panel itself
        // UIManager.Instance.HideTacticalView(); 
        Debug.Log("Scrying Eye has been disabled.");
    }
}