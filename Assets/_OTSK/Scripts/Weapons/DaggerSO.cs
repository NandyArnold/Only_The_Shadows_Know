using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Dagger", menuName = "Only The Shadows Know/Weapons/Dagger")]
public class DaggerSO : WeaponSO
{
    [Header("Dagger Specifics")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Damage Profiles")]
    [Tooltip("Damage dealt by a standard primary (LMB) or non-finisher secondary (RMB) attack.")]
    public List<DamageInstance> slashDamageProfile;
    [Tooltip("Damage dealt by a secondary (RMB) attack on an un-alerted enemy.")]
    public List<DamageInstance> finisherDamageProfile;

    // Primary Attack (LMB) - Always a standard slash.
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        //if ((combatController.GetComponent<DaggerAnimation>() != null))
        //{
        //    //combatController.GetComponent<DaggerAnimation>().PlayPrimaryAttack();
            
        //}
            combatController.PlayerAnimationController.TriggerPrimaryAttack();
            combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketL.position, attackRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var hit in hits)
            {
                // First, check if it's a real enemy with full components.
                if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(slashDamageProfile, combatController.gameObject);
                }
                // If it's not a real enemy, THEN check if it's a test dummy.
                else if (hit.TryGetComponent<DamageableDummy>(out var dummyHealth))
                {
                    if (slashDamageProfile != null && slashDamageProfile.Count > 0)
                    {
                        dummyHealth.TakeDamage(slashDamageProfile[0].Value);
                    }
                }
            }
        }
    }

    // Secondary Attack (RMB) - A finisher if the enemy is unaware.
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        
        combatController.HealthManaNoise.GenerateNoise(combatController.NoiseSettings.daggerAttackNoise);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, attackRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var hit in hits)
            {
                // First, check if it's a REAL enemy with an AI state.
                if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth) &&
                    hit.TryGetComponent<EnemyAI>(out var enemyAI))
                {
                    if (enemyAI.CurrentState is PatrolState)
                    {
                        //Debug.Log("<color=red>FINISHER!</color> (on Unaware Enemy)");
                        //combatController.GetComponent<DaggerAnimation>()?.PlayFinisherAnimation();
                        combatController.PlayerAnimationController.TriggerDagger_Finisher();
                        enemyHealth.TakeDamage(finisherDamageProfile, combatController.gameObject);
                    }
                    else
                    {
                        Debug.Log("Enemy is alert. Performing a normal heavy slash.");
                        enemyHealth.TakeDamage(slashDamageProfile, combatController.gameObject);
                    }
                }
                // If it's NOT a real enemy, THEN check if it's our special test dummy.
                else if (hit.CompareTag("TestingDummy") && hit.TryGetComponent<DamageableDummy>(out var dummyHealth))
                {
                    Debug.Log("<color=red>FINISHER!</color> (on Test Dummy)");
                    if (finisherDamageProfile != null && finisherDamageProfile.Count > 0)
                    {
                        //combatController.GetComponent<DaggerAnimation>()?.PlayFinisherAnimation();
                        combatController.PlayerAnimationController.TriggerDagger_Finisher();
                        dummyHealth.TakeDamage(finisherDamageProfile[0].Value);
                    }
                }
            }
        }
        else
        {
                combatController.PlayerAnimationController.TriggerSecondaryAttack();
            //if(combatController.GetComponent<DaggerAnimation>() != null)
            //{
            //        combatController.GetComponent<DaggerAnimation>().PlaySecondaryAttack();

            //}
        }    
    }
}