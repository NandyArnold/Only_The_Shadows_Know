// DetectionSystem.cs
using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyConfigSO config;
    [Tooltip("The point from which the enemy 'sees'. Usually the head.")]
    [SerializeField] private Transform eyePoint;

    [Header("Settings")]
    [Tooltip("Layers that will block the enemy's line of sight (e.g., walls, obstacles).")]
    [SerializeField] private LayerMask visionBlockingLayers;

    private Transform _playerTransform;

    private void Start()
    {
        // Find the player via the GameManager
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _playerTransform = GameManager.Instance.Player.transform;
        }
    }

    public bool CanSeePlayer()
    {
        if (_playerTransform == null) return false;

        // 1. Check if the player is within detection range.
        float distanceToPlayer = Vector3.Distance(eyePoint.position, _playerTransform.position);
        if (distanceToPlayer > config.detectionRange)
        {
            return false;
        }

        // 2. Check if the player is within the vision cone angle.
        Vector3 directionToPlayer = (_playerTransform.position - eyePoint.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) > config.detectionConeAngle / 2)
        {
            return false;
        }

        // 3. Check if line of sight is blocked by an obstacle.
        if (Physics.Raycast(eyePoint.position, directionToPlayer, distanceToPlayer, visionBlockingLayers))
        {
            return false; // Something is blocking the view.
        }

        // If all checks pass, the enemy can see the player.
        // If all checks pass, draw a GREEN line and return true.
        Debug.DrawRay(eyePoint.position, directionToPlayer.normalized * distanceToPlayer, Color.green);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        // Set the color for the Gizmos
        Gizmos.color = Color.yellow;

        // Draw the detection range sphere
        Gizmos.DrawWireSphere(transform.position, config.detectionRange);

        // Calculate the directions for the cone edges
        Vector3 forward = transform.forward;
        float coneAngle = config.detectionConeAngle;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-coneAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(coneAngle / 2, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        // Draw the cone lines
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + leftRayDirection * config.detectionRange);
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + rightRayDirection * config.detectionRange);
    }

}