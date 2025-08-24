// DetectionSystem.cs
using System;
using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
 
    [Tooltip("The point from which the enemy 'sees'. Usually the head.")]
    [SerializeField] private Transform eyePoint;
    public Transform EyePoint => eyePoint;

    public LayerMask VisionBlockingLayers => visionBlockingLayers;

    [Header("Settings")]
    
    [SerializeField] private LayerMask visionBlockingLayers; //Layers that will block the enemy's line of sight (e.g., walls, obstacles).
    [SerializeField] private float noiseDecayRate = 10f; // How fast the gauge empties per second

    public event Action<Vector3> OnSoundDetected;
    public event Action<float, float> OnSoundGaugeChanged;
    public event Action<Transform> OnDeadBodySpotted;

    private EnemyConfigSO config;
    private EnemyAI _enemyAI;
    private Transform _playerTransform;
    private EnemyHealth _health;

    private float _soundGauge = 0f;
    public float SoundGauge => _soundGauge;

    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _health = GetComponent<EnemyHealth>();
    }

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
        OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
    }

    private void Update()
    {
        if (_health.IsDead) return;

        bool isInAggressiveState = (_enemyAI.CurrentState is AlertState ||
                              _enemyAI.CurrentState is CombatState ||
                              _enemyAI.CurrentState is AlarmState);

        // 2. Use this single, clear boolean to control the sound decay.
        if (!isInAggressiveState && _soundGauge > 0)
        {
            _soundGauge = Mathf.MoveTowards(_soundGauge, 0, noiseDecayRate * Time.deltaTime);
        }

        // The UI update is always sent to keep it synchronized.
        OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);

        // 3. Use the same boolean to control the dead body scan.
        if (!isInAggressiveState)
        {
            if (ScanForDeadBodies(out Transform body))
            {
                OnDeadBodySpotted?.Invoke(body);
            }
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
        var playerController = _playerTransform.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsDead)
        {
            return false; // A dead player cannot be seen as a threat.
        }

        if (playerController != null && playerController.IsInEndwalkerState)
        {
            if (config.enemyType != EnemyType.Undead)
            {
                return false;
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

 
   // NEW: This method is called by the EnemyManager.
    public void OnSoundHeard(Vector3 soundPosition, float intensity)
    {
        var navigator = GetComponent<EnemyNavigator>();
        if (navigator == null || !navigator.IsAgentReady)
        {
            // If the navigator isn't ready, we can't trust our position yet.
            return;
        }
        if (config == null)
        {
            Debug.LogError($"OnSoundHeard on {gameObject.name}: Config is NULL!", gameObject);
            return;
        }
        //Debug.Log($"<color=cyan>{gameObject.name} heard a sound at {soundPosition} with intensity {intensity}</color>", this.gameObject);
        // Don't accumulate sound if we are already fully alerted or in combat
        if (_enemyAI.CurrentState is AlertState || _enemyAI.CurrentState is CombatState) return;
        // Check if the sound is within hearing range and loud enough.
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        Debug.Log($"<color=cyan>OnSoundHeard on {gameObject.name}:</color> " +
              $"Sound at {soundPosition} (Intensity: {intensity}). " +
              $"My position is {transform.position}. " +
              $"Distance to sound is {distanceToSound}. " +
              $"My hearing range is {config.hearingRange}.");
        if (distanceToSound > config.hearingRange)
        {
            Debug.LogWarning($"--> REJECTED: Sound is out of range.");
            return;
        }

        // Add the noise intensity to the gauge
        _soundGauge += intensity;
        OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
        //Debug.Log($"Enemy heard sound. Gauge is now at: {_soundGauge} / {config.hearingThreshold}");

        // If the gauge is full, fire the event
        if (_soundGauge >= config.hearingThreshold)
        {
          

            OnSoundDetected?.Invoke(soundPosition);
            _soundGauge = config.hearingThreshold; // Clamp to full instead of resetting
            OnSoundGaugeChanged?.Invoke(_soundGauge, config.hearingThreshold);
        }
    }

    public bool ScanForDeadBodies(out Transform bodyTransform)
    {
        bodyTransform = null;
        if (config == null) return false;

        Collider[] hits = Physics.OverlapSphere(transform.position, config.visionRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("DeadBody"))
            {
                //Debug.Log($"<color=cyan>[ScanForDeadBodies]</color> Found a collider with the 'DeadBody' tag: {hit.gameObject.name}");
                Vector3 directionToBody = hit.transform.position - eyePoint.position;
                //  ANGLE CHECK
                float angleToBody = Vector3.Angle(eyePoint.forward, directionToBody);
                if (angleToBody < config.detectionConeAngle / 2)
                {
                    //  LINE OF SIGHT CHECK
                    if (!Physics.Linecast(eyePoint.position, hit.transform.position + Vector3.up, visionBlockingLayers))
                    {
                        //Debug.Log("<color=green>[ScanForDeadBodies] SUCCESS:</color> Dead body is in range, in angle, and has line of sight.");
                        bodyTransform = hit.transform;
                        return true; // We can see the dead body!
                    }
                    else
                    {
                        //Debug.LogWarning($"<color=orange>[ScanForDeadBodies] FAILED:</color> Line of sight to dead body '{hit.gameObject.name}' is blocked.");
                    }
                }
                else
                {
                    //Debug.LogWarning($"<color=orange>[ScanForDeadBodies] FAILED:</color> Angle Check. Angle to body was {angleToBody}, but cone is {config.detectionConeAngle / 2}.");
                }
            }
        }
        return false; // Found nothing
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

}