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

    // Runtime variables
    private GameObject _vfxInstance;
    private EnemyHealth _targetEnemyHealth;

    // This skill is channeled, so Activate() is not used.
    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster)
    {
        // 1. Find a Target
        _targetEnemyHealth = FindClosestTarget(caster);

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

            // 4. Wait for the channel to complete
            yield return new WaitForSeconds(channelDuration);

            // 5. Execute the Finisher
            if (_targetEnemyHealth != null && Vector3.Distance(caster.transform.position, _targetEnemyHealth.transform.position) <= maxTargetingRange)
            {
                Debug.Log($"<color=purple>Death Zone Finisher</color> on {_targetEnemyHealth.name}!");
                _targetEnemyHealth.TakeDamage(damageProfile, caster);
            }
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