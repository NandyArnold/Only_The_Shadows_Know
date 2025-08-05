using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Animancy", menuName = "Only The Shadows Know/Weapons/Animancy")]
public class AnimancySO : WeaponSO
{
    [Header("Ranged Attack (LMB)")]
    [SerializeField] private GameObject beamVFXPrefab;
    [SerializeField] private float rangedRange = 100f;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private List<DamageInstance> rangedDamageProfile;

    [Header("Melee Attack (RMB)")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private List<DamageInstance> meleeDamageProfile;

    [Header("Execution Mechanic")]
    [Tooltip("Enemy types in this list will receive additional 'Execution' damage from all attacks.")]
    [SerializeField] private List<EnemyType> vulnerableToExecution;
    [SerializeField] private List<DamageInstance> executionDamageProfile;

    // Ranged Soul Sever (LMB)
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerPrimaryAttack();
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise); // Placeholder

        Transform firePoint = combatController.FirePoint;
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Find where the player is aiming from the center of the screen
        Vector3 targetPoint;
        Ray screenCenterRay = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(screenCenterRay, out RaycastHit screenHit, rangedRange, aimLayerMask))
        {
            targetPoint = screenHit.point;
        }
        else
        {
            targetPoint = screenCenterRay.GetPoint(rangedRange);
        }

        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;
        Vector3 endPoint = firePoint.position + fireDirection * rangedRange;

        // Perform the actual hitscan raycast from the fire point to deal damage
        if (Physics.Raycast(firePoint.position, fireDirection, out RaycastHit damageHit, rangedRange, hittableLayers))
        {
            endPoint = damageHit.point; // Make the beam stop where it hit
            combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

            if (damageHit.collider.TryGetComponent<EnemyHealth>(out var enemyHealth) && damageHit.collider.TryGetComponent<Enemy>(out var enemy))
            {
                // 1. Apply the standard ranged damage
                enemyHealth.TakeDamage(rangedDamageProfile, combatController.gameObject);

                // 2. Check for and apply additional execution damage
                if (vulnerableToExecution.Contains(enemy.Config.enemyType))
                {
                    enemyHealth.TakeDamage(executionDamageProfile, combatController.gameObject);
                }
            }
        }

        // Spawn the visual beam effect
        if (beamVFXPrefab != null)
        {
            GameObject beamObject = Instantiate(beamVFXPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
            LineRenderer line = beamObject.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, new Vector3(0, 0, Vector3.Distance(firePoint.position, endPoint)));
            }
        }
    }

    // Melee Soul Sever (RMB)
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerSecondaryAttack();
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, meleeRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var h in hits)
            {
                if (h.TryGetComponent<EnemyHealth>(out var enemyHealth) && h.TryGetComponent<Enemy>(out var enemy))
                {
                    // 1. Apply the standard melee damage
                    enemyHealth.TakeDamage(meleeDamageProfile, combatController.gameObject);

                    // 2. Check for and apply additional execution damage
                    if (vulnerableToExecution.Contains(enemy.Config.enemyType))
                    {
                        enemyHealth.TakeDamage(executionDamageProfile, combatController.gameObject);
                    }
                }
            }
        }
    }
}