using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Animancy", menuName = "Only The Shadows Know/Weapons/Animancy")]
public class AnimancySO : WeaponSO
{
    [Header("Ranged Attack (LMB)")]
    [SerializeField] private GameObject beamVFXPrefab;
    [SerializeField] private float beamDuration; // Duration for the beam effect
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
        NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

        Transform firePoint = combatController.FirePoint;
        CinemachineBrain cinemachineBrain = combatController.Brain;
        if (cinemachineBrain == null) return;

        // Get the active camera
        Camera activeCamera = cinemachineBrain.OutputCamera;

        // 1. Find where the player is aiming at the center of the screen
        Vector3 targetPoint;
        Ray screenCenterRay = activeCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
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

        // 2. Perform the actual hitscan raycast from the fire point to deal damage
        if (Physics.Raycast(firePoint.position, fireDirection, out RaycastHit damageHit, rangedRange, hittableLayers))
        {
            endPoint = damageHit.point; // The beam should end exactly where the damage was dealt.

            NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

            // Check if the hit object is a real enemy
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
            // Check if the hit object is a test dummy
            else if (damageHit.collider.TryGetComponent<DamageableDummy>(out var dummyHealth))
            {
                if (rangedDamageProfile != null && rangedDamageProfile.Count > 0)
                {
                    dummyHealth.TakeDamage(rangedDamageProfile[0].Value);
                }
            }
        }

        // 3. Spawn the visual beam effect and set its world positions
        if (beamVFXPrefab != null)
        {
            // We spawn it at the fire point, but its rotation doesn't matter now.
            GameObject beamObject = Instantiate(beamVFXPrefab, firePoint.position, Quaternion.identity);
            LineRenderer line = beamObject.GetComponent<LineRenderer>();
            if (line != null)
            {
                // THE FIX: Set the line's start and end points in absolute WORLD space.
                line.SetPosition(0, firePoint.position);
                line.SetPosition(1, endPoint);
            }
            if (beamObject.TryGetComponent<VFXCleanup>(out var cleanup))
            {
                // You can make this duration a variable in the SO later if you want.
                cleanup.BeginCleanup(beamDuration);
            }
        }
    }

    // Melee Soul Sever (RMB)
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerSecondaryAttack();
        NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, meleeRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var h in hits)
            {
                // --- THIS IS THE FIX ---
                // First, check if it's a real enemy
                if (h.TryGetComponent<EnemyHealth>(out var enemyHealth) && h.TryGetComponent<Enemy>(out var enemy))
                {
                    enemyHealth.TakeDamage(meleeDamageProfile, combatController.gameObject);
                    if (vulnerableToExecution.Contains(enemy.Config.enemyType))
                    {
                        enemyHealth.TakeDamage(executionDamageProfile, combatController.gameObject);
                    }
                }
                // If not, check if it's a test dummy
                else if (h.TryGetComponent<DamageableDummy>(out var dummyHealth))
                {
                    if (meleeDamageProfile != null && meleeDamageProfile.Count > 0)
                    {
                        dummyHealth.TakeDamage(meleeDamageProfile[0].Value);
                    }
                }
            }
        }
    }
}
