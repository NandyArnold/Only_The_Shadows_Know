// DaggerSO.cs - UPGRADED

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Dagger", menuName = "Only The Shadows Know/Weapons/Dagger")]
public class DaggerSO : WeaponSO
{
    [Header("Dagger Attack Settings")]
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Primary Attack Damage")]
    public List<DamageInstance> primaryDamageProfile;

    [Header("Finisher Damage")]
    public List<DamageInstance> finisherDamageProfile;

    // The Primary Attack uses the LEFT hand.
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        // 1. Tell the animator to play the attack.
        combatController.PlayerAnimationController.TriggerPrimaryAttack();
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

        // 2. Perform the hit detection.
        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketL.position, attackRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var hit in hits)
            {
                Debug.Log($"<color=green>Dagger (R):</color> Hit {hit.gameObject.name} for {primaryDamageProfile} damage.");
                if (hit.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
                {
                 
                    enemyHealth.TakeDamage(primaryDamageProfile,combatController.gameObject);
                }
            }
        }
    }

    // The Secondary Attack uses the RIGHT hand.
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        combatController.PlayerAnimationController.TriggerSecondaryAttack();

        // Noise is generated whether we hit or not.
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, attackRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var hit in hits)
            {
                // We need both health and AI components from the enemy.
                if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth) &&
                    hit.TryGetComponent<EnemyAI>(out var enemyAI))
                {
                    // THE FINISHER CHECK: Is the enemy in its PatrolState?
                    if (enemyAI.CurrentState is PatrolState)
                    {
                        Debug.Log("<color=red>FINISHER!</color>");
                        enemyHealth.TakeDamage(finisherDamageProfile, combatController.gameObject);
                    }
                    else
                    {
                        // If the enemy is alerted or in combat, just do a normal slash.
                        Debug.Log("Enemy is alert. Performing a normal heavy slash.");
                        enemyHealth.TakeDamage(primaryDamageProfile, combatController.gameObject);
                    }
                }
            }
        }
    }
}