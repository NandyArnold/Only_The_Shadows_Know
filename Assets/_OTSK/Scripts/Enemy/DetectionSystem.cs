// DetectionSystem.cs
using System;
using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyConfigSO config;
    [Tooltip("The point from which the enemy 'sees'. Usually the head.")]
    [SerializeField] private Transform eyePoint;

    [Header("Settings")]
    
    [SerializeField] private LayerMask visionBlockingLayers; //Layers that will block the enemy's line of sight (e.g., walls, obstacles).
    [SerializeField] private float noiseDecayRate = 10f; // How fast the gauge empties per second

    public event Action<Vector3> OnSoundDetected;
    public event Action<float, float> OnSoundGaugeChanged;

    private Transform _playerTransform;
    private float _soundGauge = 0f;

    private void Start()
    {
        // Find the player via the GameManager
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _playerTransform = GameManager.Instance.Player.transform;
        }
    }

    private void Update()
    {
        // Decay the sound gauge over time
        if (_soundGauge > 0)
        {
            _soundGauge = Mathf.Max(0, _soundGauge - noiseDecayRate * Time.deltaTime);
            OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
        }
    }


    public bool CanSeePlayer()
    {
        if (_playerTransform == null) return false;

        // 1. Check if the player is within detection range.
        float distanceToPlayer = Vector3.Distance(eyePoint.position, _playerTransform.position);
        if (distanceToPlayer > config.visionRange)
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
        Gizmos.DrawWireSphere(transform.position, config.visionRange);

        // Calculate the directions for the cone edges
        Vector3 forward = transform.forward;
        float coneAngle = config.detectionConeAngle;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-coneAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(coneAngle / 2, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        // Draw the cone lines
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + leftRayDirection * config.visionRange);
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + rightRayDirection * config.visionRange);
    }


    // NEW: This method is called by the EnemyManager.
    public void OnSoundHeard(Vector3 soundPosition, float intensity)
    {
        // Check if the sound is within hearing range and loud enough.
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        if (distanceToSound > config.hearingRange) return;

        // Add the noise intensity to the gauge
        _soundGauge += intensity;
        OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
        //Debug.Log($"Enemy heard sound. Gauge is now at: {_soundGauge} / {config.hearingThreshold}");

        // If the gauge is full, fire the event
        if (_soundGauge >= config.hearingThreshold)
        {
            OnSoundDetected?.Invoke(soundPosition);
            _soundGauge = 0f; // Reset the gauge after being alerted
            OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
        }
    }

}