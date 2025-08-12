using UnityEngine;
using Unity.Cinemachine;

public class AimTargetController : MonoBehaviour
{
    [SerializeField] private LayerMask aimLayerMask;
    private CinemachineBrain _cinemachineBrain;

    private void Awake()
    {
        // Subscribe to game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        DontDestroyOnLoad(gameObject);
    }

    //private void Start()
    //{
    //    // It's safer to get the brain in Start, after all Awakes have run.
    //    if (Camera.main != null)
    //    {
    //        _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    //    }

    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.RegisterAimTarget(transform);
    //    }
    //}

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        // Only enable this script's Update method during active gameplay.
        this.enabled = (newState == GameState.Gameplay);
    }

    void LateUpdate()
    {
        if (_cinemachineBrain == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                // Ask the PlayerCombat script for its brain
                _cinemachineBrain = GameManager.Instance.Player.GetComponent<PlayerCombat>().Brain;
            }

            // If we still can't find it, exit to prevent errors.
            if (_cinemachineBrain == null) return;
        }

        Camera activeCamera = _cinemachineBrain.OutputCamera;
        if (activeCamera == null) return;

        // The raycast logic is now guaranteed to use the correct camera.
        Ray ray = new Ray(activeCamera.transform.position, activeCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = ray.GetPoint(100f);
        }
    }
}