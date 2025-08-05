// AnimancySO.cs - REFACTORED for Hitscan
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Animancy", menuName = "Only The Shadows Know/Weapons/Animancy")]
public class AnimancySO : WeaponSO
{
    [Header("Ranged Attack (Hitscan)")]
    [SerializeField] private GameObject beamVFXPrefab; // The beam effect we just made
    [SerializeField] private float rangedDamage = 20f;
    [SerializeField] private float rangedRange = 100f;
    [SerializeField] private LayerMask aimLayerMask;

    [Header("Melee Attack")]
    [SerializeField] private float meleeDamage = 30f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Specialized Damage")]
    [Tooltip("Define damage multipliers for specific enemy types.")]
    [SerializeField] private List<DamageMultiplier> damageMultipliers;

    // Ranged Soul Sever (LMB)
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerPrimaryAttack();
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.animancyRangedAttackNoise);

        Transform firePoint = combatController.FirePoint;
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Find where the player is aiming
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

        // Calculate the direction from the fire point to the aim target
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        // Spawn the visual beam effect
        if (beamVFXPrefab != null)
        {
            GameObject beamObject = Instantiate(beamVFXPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
            LineRenderer line = beamObject.GetComponent<LineRenderer>();
            line.SetPosition(0, Vector3.zero); // Start of the line at the fire point
            line.SetPosition(1, new Vector3(0, 0, rangedRange)); // End of the line stretches out
        }

        // Perform the actual hitscan raycast
        if (Physics.Raycast(firePoint.position, fireDirection, out RaycastHit damageHit, rangedRange, hittableLayers))
        {
            if (damageHit.collider.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(rangedDamage, combatController.gameObject);
            }
        }
    }

    // Melee Soul Sever (RMB) - This logic remains unchanged.
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerSecondaryAttack();
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.animancyMeleeAttackNoise);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, meleeRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var h in hits)
            {
                if (h.TryGetComponent<EnemyHealth>(out var enemyHealth) && h.TryGetComponent<Enemy>(out var enemy))
                {
                    // Calculate final damage with multiplier
                    float multiplier = GetMultiplierFor(enemy.Config.enemyType);
                    float finalDamage = meleeDamage * multiplier;
                    enemyHealth.TakeDamage(finalDamage, combatController.gameObject);
                }
            }
        }
    }

    //  Helper method to find the correct multiplier.
    private float GetMultiplierFor(EnemyType type)
    {
        var multiplier = damageMultipliers.FirstOrDefault(m => m.enemyType == type);
        return multiplier != null ? multiplier.multiplier : 1f; // Default to 1x damage
    }
}