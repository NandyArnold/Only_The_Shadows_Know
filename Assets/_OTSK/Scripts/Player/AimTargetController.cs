using UnityEngine;
using Unity.Cinemachine;

public class AimTargetController : MonoBehaviour
{
    public static AimTargetController Instance { get; private set; }
    public Camera ActiveAimCamera { get; private set; }

    [SerializeField] private LayerMask aimLayerMask;
    [Tooltip("The fixed distance to place the IK target. Keeps animations smooth.")]
    [SerializeField] private float stableAimDistance = 100f;

    // Public properties for other scripts to read
    public bool HasHit { get; private set; }
    public RaycastHit CurrentHit { get; private set; }

    private CinemachineBrain _cinemachineBrain;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Subscribe to game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Tell the GameManager that this is the active AimTargetController
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAimTarget(this);
        }
    }

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
            if (GameManager.Instance?.Player != null)
                _cinemachineBrain = GameManager.Instance.Player.GetComponent<PlayerCombat>().Brain;

            if (_cinemachineBrain == null) return;
        }

        ActiveAimCamera = _cinemachineBrain.OutputCamera;
        if (ActiveAimCamera == null) return;

        // --- Perform the Aim Calculation ---
        Ray ray = new Ray(ActiveAimCamera.transform.position, ActiveAimCamera.transform.forward);

        // The IK target is always placed at a stable, distant point.
        transform.position = ray.GetPoint(stableAimDistance);

        // The gameplay raycast finds what we're *actually* looking at.
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
        {
            // Failsafe in case player isn't ready yet
            if (GameManager.Instance.Player == null) return;

            Transform playerTransform = GameManager.Instance.Player.transform;
            Vector3 playerForward = playerTransform.forward;
            Vector3 directionToHit = hit.point - playerTransform.position;

            // --- THIS IS THE FIX ---
            // Only consider hits that are in front of the player's body.
            if (Vector3.Dot(playerForward, directionToHit) > 0)
            {
                // This is a valid hit in front of us.
                HasHit = true;
                CurrentHit = hit;
            }
            else
            {
                // The hit was BEHIND the player, so we ignore it.
                HasHit = false;
            }
        }
        else
        {
            HasHit = false;
        }
    }
}
