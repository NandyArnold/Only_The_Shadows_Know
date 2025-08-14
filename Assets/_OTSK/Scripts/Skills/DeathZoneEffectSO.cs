using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FX_DeathZone", menuName = "Only The Shadows Know/Skills/Effects/Death Zone Effect")]
public class DeathZoneEffectSO : SkillEffectSO
{
    [Header("Death Zone Settings")]
    [SerializeField] private GameObject deathZoneVFXPrefab;
    [Tooltip("How long the player must channel to execute the finisher.")]
    [SerializeField] private float channelDuration = 2f;
    [Tooltip("The maximum range to find a target from the player.")]
    [SerializeField] private float maxTargetingRange = 5f;
    [Tooltip("Layers that contain valid targets (e.g., 'Hittable').")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Damage Profile")]
    [SerializeField] private List<DamageInstance> damageProfile;

   


    public float MaxTargetingRange => maxTargetingRange;
    public LayerMask TargetLayers => targetLayers;

    // Runtime variables
    private GameObject _vfxInstance;
    private EnemyHealth _targetEnemyHealth;

    // This skill is channeled, so Activate() is not used.
    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster, object targetPayload)
    {
        // 1. Find a Target
        Enemy target = targetPayload as Enemy;
        if (target == null)
        {
            Debug.LogError("Death Zone was started without a valid Enemy target!");
            caster.GetComponent<PlayerSkillController>().OnSkillEffectFinished();
            yield break;
        }

        _targetEnemyHealth = target.GetComponent<EnemyHealth>();

        if (_targetEnemyHealth == null)
        {
            Debug.Log("Death Zone failed: No valid target in range.");
            caster.GetComponent<PlayerSkillController>().CancelChannel();
            yield break;
        }

        // Use a try...finally block to guarantee cleanup
        try
        {
            // 2. Find the socket on the target
            Transform vfxAnchor = FindDeepChild(_targetEnemyHealth.transform, "DeathZoneSocket") ?? _targetEnemyHealth.transform;

            // 3. Spawn VFX and parent it to the anchor
            if (deathZoneVFXPrefab != null)
            {
                _vfxInstance = Instantiate(deathZoneVFXPrefab, vfxAnchor.position, vfxAnchor.rotation, vfxAnchor);
                if (_vfxInstance.TryGetComponent<DeathZoneController>(out var controller))
                {
                    controller.Initialize(caster.GetComponent<PlayerCombat>().HandSocketR, vfxAnchor);
                }
            }

            float channelTimer = 0f;
        while (channelTimer < channelDuration)
        {
            // Check if the target is still valid every frame
            if (_targetEnemyHealth == null || Vector3.Distance(caster.transform.position, _targetEnemyHealth.transform.position) > maxTargetingRange)
            {
                Debug.Log("Death Zone cancelled: Target moved out of range.");
                    
                    yield break; // Exit the coroutine, the finally block will handle cleanup
            }

            // Check for line of sight every frame
            Transform playerEyes = caster.GetComponent<CameraController>().transform;
            Vector3 enemyCenter = _targetEnemyHealth.transform.position + Vector3.up;
            if (Physics.Linecast(playerEyes.position, enemyCenter, caster.GetComponent<PlayerCombat>().CurrentWeapon.LineOfSightBlockingLayers))
            {
                Debug.Log("Death Zone cancelled: Line of sight broken.");
                   
                    yield break; // Exit the coroutine
            }

            channelTimer += Time.deltaTime;
            yield return null;
        }
        
        // If the loop completes, execute the finisher
        Debug.Log($"<color=purple>Death Zone Finisher</color> on {_targetEnemyHealth.name}!");
        _targetEnemyHealth.TakeDamage(damageProfile, caster, true); // Pass 'true' for silent kill
        }
        finally
        {
            // 6. This code will run NO MATTER WHAT (success, failure, or player cancellation).
            // It tells the controller that the channel is over, which will trigger StopChannel().
            caster.GetComponent<PlayerSkillController>().CancelChannel();
        }
    }

    public override void StopChannel(GameObject caster)
    {
        // Clean up the VFX if the channel is stopped early or completes.
        if (_vfxInstance != null)
        {
            Destroy(_vfxInstance);
        }
        _targetEnemyHealth = null;
        caster.GetComponent<PlayerAnimationController>().CancelCurrentAnimation();

        Debug.Log("Stopped channeling Death Zone.");
    }

    private EnemyHealth FindClosestTarget(GameObject caster)
    {
        Collider[] hits = Physics.OverlapSphere(caster.transform.position, maxTargetingRange, targetLayers);

        EnemyHealth closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            // Ensure we don't target an already dead enemy
            if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth) && enemyHealth.CurrentHealth > 0)
            {
                float distance = Vector3.Distance(caster.transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemyHealth;
                }
            }
        }
        return closestEnemy;
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}