// DetectionSystem.cs
using System;
using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
 
    [Tooltip("The point from which the enemy 'sees'. Usually the head.")]
    [SerializeField] private Transform eyePoint;

    [Header("Settings")]
    
    [SerializeField] private LayerMask visionBlockingLayers; //Layers that will block the enemy's line of sight (e.g., walls, obstacles).
    [SerializeField] private float noiseDecayRate = 10f; // How fast the gauge empties per second

    public event Action<Vector3> OnSoundDetected;
    public event Action<float, float> OnSoundGaugeChanged;

    private EnemyConfigSO config;
    private Transform _playerTransform;
    private float _soundGauge = 0f;

    public void Initialize(EnemyConfigSO newConfig)
    {
        config = newConfig;
    }
    private void Start()
    {
        // Find the player via the GameManager
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            _playerTransform = GameManager.Instance.Player.transform;
            //Debug.Log($"DetectionSystem: Player Transform found: {_playerTransform.name}");
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
        if (_playerTransform == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                Debug.LogWarning("DetectionSystem: Player Transform was null, but GameManager found the player. Updating reference.");
                _playerTransform = GameManager.Instance.Player.transform;
            }
            else
            {
                 Debug.LogWarning("DetectionSystem: Player Transform is not set. Cannot perform vision check.");
                 return false;
                
            }
        }
        if (_playerTransform.TryGetComponent<PlayerController>(out var playerController))
        {
            if (playerController.IsInEndwalkerState)
            {
                if (config.enemyType != EnemyType.Undead)
                {
                    return false;
                }
            }
        }

        Vector3 directionToPlayer = _playerTransform.position - eyePoint.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 1. Check if the player is within detection range.

        if (distanceToPlayer > config.visionRange)
        {
            //Debug.Log($"<color=orange>VISION FAILED:</color> Distance Check. Distance was {distanceToPlayer}, but range is {config.visionRange}.");
            return false;
            
        }

        // 2. Check the angle on the horizontal (XZ) plane, ignoring height differences.
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(eyePoint.forward, Vector3.up).normalized;
        Vector3 directionOnPlane = Vector3.ProjectOnPlane(directionToPlayer, Vector3.up).normalized;
      //-------- debuglog 
        float horizontalAngle = Vector3.Angle(forwardOnPlane, directionOnPlane);

        if (horizontalAngle > config.detectionConeAngle / 2)
        {
            //Debug.Log($"<color=orange>VISION FAILED:</color> Angle Check. Angle was {horizontalAngle}, but cone is {config.detectionConeAngle / 2}.");
            return false;
        }
      //-----------
        if (Vector3.Angle(forwardOnPlane, directionOnPlane) > config.detectionConeAngle / 2)
        {
            return false;
        }

        // 3. Check if line of sight is blocked by an obstacle in full 3D.
        // We aim for the player's center mass for a more reliable check.
        Vector3 playerCenterMass = _playerTransform.position + Vector3.up * 1.5f;
        if (Physics.Linecast(eyePoint.position, playerCenterMass, out RaycastHit hit, visionBlockingLayers))
        {
            //Debug.Log($"<color=orange>VISION FAILED:</color> Line of sight blocked by '{hit.collider.name}'.");
            return false;
        }

        // If all checks pass, the enemy can see the player.
        //Debug.Log("<color=green>VISION SUCCESS:</color> Player is visible.");
        Debug.DrawRay(eyePoint.position, directionToPlayer * distanceToPlayer, Color.green);
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