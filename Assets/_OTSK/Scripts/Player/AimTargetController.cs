using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    [SerializeField] private LayerMask aimLayerMask;
    private Camera _mainCamera;
    private MeshRenderer _renderer; // Optional: for a visual debug marker

    private void Awake()
    {
        _mainCamera = Camera.main;
        _renderer = GetComponent<MeshRenderer>();

        // Subscribe to game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }
    private void Start()
    {
        _mainCamera = Camera.main;
        // ADD THIS LINE: Register with the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAimTarget(transform);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    // This will now control whether the script is active
    private void HandleGameStateChanged(GameState newState)
    {
        // Only enable the aim target when in active gameplay
        bool shouldBeActive = (newState == GameState.Gameplay);
        if (_renderer != null) _renderer.enabled = shouldBeActive; // Hide debug marker if we have one
        this.enabled = shouldBeActive; // Enable/disable this script's Update method
    }

    void Update()
    {
        Ray ray = _mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
            transform.position = hit.point;
        else
            transform.position = ray.GetPoint(100f);
    }
}