using System.Collections;
using System.Collections.Generic; // Required for List
using UnityEngine;

[CreateAssetMenu(fileName = "FX_DeathZone", menuName = "Only The Shadows Know/Skills/Effects/Death Zone Effect")]
public class DeathZoneEffectSO : SkillEffectSO
{
    [Header("Death Zone Settings")]
    [SerializeField] private GameObject deathZoneVFXPrefab;
    [SerializeField] private float channelDuration = 2f;
    [SerializeField] private float maxTargetingRange = 5f;
    [SerializeField] private LayerMask targetLayers;

    // REPLACE the old damage fields with this list
    [Header("Damage Profile")]
    [SerializeField] private List<DamageInstance> damageProfile;

    private GameObject _vfxInstance;
    private EnemyHealth _targetEnemyHealth;

    public override void Activate(GameObject caster) { }

    public override IEnumerator StartChannel(GameObject caster)
    {
        _targetEnemyHealth = FindClosestTarget(caster);
        if (_targetEnemyHealth == null)
        {
            caster.GetComponent<PlayerSkillController>().CancelChannel();
            yield break;
        }

        if (deathZoneVFXPrefab != null)
        {
            Transform vfxAnchor = _targetEnemyHealth.transform.Find("DeathZoneSocket") ?? _targetEnemyHealth.transform;
            _vfxInstance = Instantiate(deathZoneVFXPrefab, vfxAnchor);
            if (_vfxInstance.TryGetComponent<DeathZoneController>(out var controller))
            {
                controller.Initialize(caster.GetComponent<PlayerCombat>().HandSocketR, vfxAnchor);
            }
        }

        yield return new WaitForSeconds(channelDuration);

        if (_targetEnemyHealth != null && Vector3.Distance(caster.transform.position, _targetEnemyHealth.transform.position) <= maxTargetingRange)
        {
            // Pass the entire damage profile list to the TakeDamage method
            _targetEnemyHealth.TakeDamage(damageProfile, caster);
        }

        caster.GetComponent<PlayerSkillController>().CancelChannel();
    }

    public override void StopChannel(GameObject caster)
    {
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