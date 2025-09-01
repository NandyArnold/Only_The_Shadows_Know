// In ScryingSystem.cs
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
public class ScryingSystem : MonoBehaviour
{
    public static ScryingSystem Instance { get; private set; }

    [Header("Component References")]
    private GameObject scryingCameraObject;
    private CinemachineBrain mainCameraBrain;
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

    }

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCameraBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
        // Use a coroutine to wait for the scene's CameraManager to be ready.
        StartCoroutine(FindSceneCameraRoutine());
    }

    private IEnumerator FindSceneCameraRoutine()
    {
        // Wait until the CameraManager for the current scene has initialized.
        yield return new WaitUntil(() => CameraManager.Instance != null);

        // Get the CinemachineCamera component from the manager
        CinemachineCamera vcam = CameraManager.Instance.GetCamera(CameraType.Scrying);

        if (vcam != null)
        {
            // As per our setup, the vcam is a child of the main render camera object.
            // We get its parent to control the whole camera rig.
            scryingCameraObject = vcam.transform.parent.gameObject;
            Debug.Log("ScryingSystem successfully linked to scene camera.");

            // Ensure the eye is off at the start
            if (scryingCameraObject != null) scryingCameraObject.SetActive(false);
            IsScryingDeployed = false;
        }
        else
        {
            Debug.LogError("ScryingSystem could not find the Scrying camera in the CameraManager!");
        }
    }

    public void DeployScryingEye(Vector3 position)
    {
        if (IsScryingDeployed || scryingCameraObject == null) return;

        // *** STEP 2: Disable the main brain BEFORE activating the scrying camera ***
        if (mainCameraBrain != null)
        {
            mainCameraBrain.enabled = false;
        }

        scryingCameraObject.transform.position = position + new Vector3(0, 50, 0);
        scryingCameraObject.SetActive(true);
        IsScryingDeployed = true;

        HUDManager.Instance.ShowMinimap(minimapTexture);
        Debug.Log("Scrying Eye deployed, main brain disabled.");
    }

    public void DisableScryingEye()
    {
        if (!IsScryingDeployed || scryingCameraObject == null) return;

        scryingCameraObject.SetActive(false);
        IsScryingDeployed = false;

        // *** STEP 3: Re-enable the main brain to return to normal gameplay ***
        if (mainCameraBrain != null)
        {
            mainCameraBrain.enabled = true;
        }

        HUDManager.Instance.HideMinimap();
        Debug.Log("Scrying Eye disabled, main brain enabled.");
    }
}