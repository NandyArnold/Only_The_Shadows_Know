using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FX_DeathZone", menuName = "Only The Shadows Know/Skills/Effects/Death Zone Effect")]
public class DeathZoneEffectSO : SkillEffectSO
{
    [Header("Death Zone Settings")]
    [SerializeField] private GameObject deathZoneVFXPrefab;
    [Tooltip("How long the player must channel to execute the finisher.")]
    [SerializeField] private List<DamageInstance> damageProfile;
    [SerializeField] private DamageTypeSO damageType;
    [SerializeField] private float channelDuration = 2f;
    [Tooltip("The maximum range to find a target from the player.")]
    [SerializeField] private float maxTargetingRange = 5f;
    [Tooltip("Layers that contain valid targets.")]
    [SerializeField] private LayerMask targetLayers;

    private GameObject _vfxInstance;
    private EnemyHealth _targetEnemyHealth;
    
    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster)
    {
        // 1. Find a Target
        _targetEnemyHealth = FindClosestTarget(caster);

        if (_targetEnemyHealth == null)
        {
            Debug.Log("Death Zone failed: No valid target in range.");
            // We need to tell the controller to stop the channel immediately
            caster.GetComponent<PlayerSkillController>().CancelChannel();
            yield break;
        }

        // 2. Find the socket on the target
        Transform vfxAnchor = _targetEnemyHealth.transform.Find("DeathZoneSocket");
        if (vfxAnchor == null)
        {
            vfxAnchor = _targetEnemyHealth.transform; // Fallback to the root if no socket found
        }
        // 3. Spawn VFX on the target
        if (deathZoneVFXPrefab != null)
        {
            _vfxInstance = Instantiate(deathZoneVFXPrefab, vfxAnchor);
            if (_vfxInstance.TryGetComponent<DeathZoneController>(out var controller))
            {
                controller.Initialize(caster.GetComponent<PlayerCombat>().HandSocketR, vfxAnchor);
            }
        }

        // 3. Wait for the channel to complete
        yield return new WaitForSeconds(channelDuration);

        // 4. Execute the Finisher
        // Check if the target is still valid (not dead) and in range
        if (_targetEnemyHealth != null && Vector3.Distance(caster.transform.position, _targetEnemyHealth.transform.position) <= maxTargetingRange)
        {
            Debug.Log($"<color=purple>Death Zone Finisher</color> on {_targetEnemyHealth.name}!");
            
            _targetEnemyHealth.TakeDamage(damageProfile, caster); // Overkill damage for a finisher
        }

        // 5. Tell the controller the channel is complete
        caster.GetComponent<PlayerSkillController>().CancelChannel();
    }

    public override void StopChannel(GameObject caster)
    {
        // Clean up the VFX if the channel is stopped early
        if (_vfxInstance != null)
        {
            Destroy(_vfxInstance);
        }
        _targetEnemyHealth = null;
    }

    private EnemyHealth FindClosestTarget(GameObject caster)
    {
        Collider[] hits = Physics.OverlapSphere(caster.transform.position, maxTargetingRange, targetLayers);

        EnemyHealth closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth))
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
}