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

    [Header("Dagger Specific Audio")]
    [Tooltip("Sound for a successful finisher attack. This is not surface-dependent.")]
    public SoundDefinition finisherSound;
    [Tooltip("Sound for when the dagger swings and hits nothing.")]
    public SoundDefinition swingSound;


    // Primary Attack (LMB) - Always a standard slash.
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        //if ((combatController.GetComponent<DaggerAnimation>() != null))
        //{
        //    //combatController.GetComponent<DaggerAnimation>().PlayPrimaryAttack();
            
        //}
        combatController.PlayerAnimationController.TriggerPrimaryAttack();
        NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketL.position, attackRange, hittableLayers);
        if (hits.Any())
        {

            var primaryHit = hits[0];
            SurfaceType surface = SurfaceType.Default;
            if (primaryHit.TryGetComponent<SurfaceIdentifier>(out var identifier))
            {
                surface = identifier.surfaceType;
            }
            GetImpactSound(surface).Play(primaryHit.transform); // Uses the inherited method


            foreach (var hit in hits)
            {

                // First, check if it's a real enemy with full components.
                if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(slashDamageProfile, combatController.gameObject);
                }
                // If it's NOT a real enemy, THEN check if it's a destructible object.
                else if (hit.TryGetComponent<Destructible>(out var destructible))
                {
                    destructible.TakeDamage(slashDamageProfile, combatController.gameObject);
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
        else
        {
            swingSound.Play(combatController.transform);
        }
    }
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, attackRange, hittableLayers);

        // 1. Handle the "miss" case first for clarity.
        if (!hits.Any())
        {
            swingSound.Play(combatController.transform);
            combatController.PlayerAnimationController.TriggerSecondaryAttack();
            return; // Exit the method since we missed
        }

        // 2. If we hit something, determine the attack type ONCE.
        bool isFinisher = false;
        // Scan all hits to see if ANY of them qualify for a finisher.
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyAI>(out var enemyAI) &&
               (enemyAI.CurrentState is PatrolState || enemyAI.CurrentState is GuardState))
            {
                isFinisher = true;
                break; // We found a finisher target, no need to check further.
            }
        }

        // 3. Now, play the correct sound and trigger the correct animation ONCE.
        var primaryHit = hits[0]; // For getting the surface type of the first thing hit
        SurfaceType surface = SurfaceType.Default;
        if (primaryHit.TryGetComponent<SurfaceIdentifier>(out var identifier))
        {
            surface = identifier.surfaceType;
        }

        if (isFinisher)
        {
            // Play finisher sound (not surface-dependent)
            finisherSound.Play(primaryHit.transform);
            combatController.PlayerAnimationController.TriggerDagger_Finisher();
        }
        else
        {
            // It's a normal slash, so play the surface-dependent impact sound
            GetImpactSound(surface).Play(primaryHit.transform);
            combatController.PlayerAnimationController.TriggerSecondaryAttack();
        }

        // 4. Finally, loop through all hits to apply the correct damage.
        foreach (var hit in hits)
        {
            List<DamageInstance> damageToApply = isFinisher ? finisherDamageProfile : slashDamageProfile;

            if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damageToApply, combatController.gameObject);
            }
            else if (hit.TryGetComponent<Destructible>(out var destructible))
            {
                destructible.TakeDamage(damageToApply, combatController.gameObject);
            }
            else if (hit.TryGetComponent<DamageableDummy>(out var dummyHealth))
            {
                if (damageToApply != null && damageToApply.Count > 0)
                {
                    dummyHealth.TakeDamage(damageToApply[0].Value);
                }
            }
        }
    }
    // Secondary Attack (RMB) - A finisher if the enemy is unaware.
    //public override void SecondaryAttack(PlayerCombat combatController)
    //{

    //    NoiseManager.Instance.GenerateNoise(combatController.transform.position, combatController.NoiseSettings.daggerAttackNoise, combatController.gameObject);

    //    Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, attackRange, hittableLayers);
    //    if (hits.Any())
    //    {
    //        var primaryHit = hits[0];
    //        foreach (var hit in hits)
    //        {
    //            // First, check if it's a REAL enemy with an AI state.
    //            if (hit.TryGetComponent<EnemyHealth>(out var enemyHealth) &&
    //                hit.TryGetComponent<EnemyAI>(out var enemyAI))
    //            {
    //                if (enemyAI.CurrentState is PatrolState || enemyAI.CurrentState is GuardState)
    //                {


    //                    //Debug.Log("<color=red>FINISHER!</color> (on Unaware Enemy)");
    //                    finisherSound.Play(primaryHit.transform);
    //                    combatController.PlayerAnimationController.TriggerDagger_Finisher();
    //                    enemyHealth.TakeDamage(finisherDamageProfile, combatController.gameObject);
    //                }
    //                else
    //                {
    //                    SurfaceType surface = SurfaceType.Default;
    //                    if (primaryHit.TryGetComponent<SurfaceIdentifier>(out var identifier))
    //                    {
    //                        surface = identifier.surfaceType;
    //                    }
    //                    GetImpactSound(surface).Play(primaryHit.transform);

    //                    combatController.PlayerAnimationController.TriggerSecondaryAttack();
    //                    Debug.Log("Enemy is alert. Performing a normal heavy slash.");
    //                    enemyHealth.TakeDamage(slashDamageProfile, combatController.gameObject);
    //                }
    //            }
    //            // If it's NOT a real enemy, THEN check if it's a destructible object.
    //            else if (hit.TryGetComponent<Destructible>(out var destructible))
    //            {
    //                combatController.PlayerAnimationController.TriggerSecondaryAttack();
    //                destructible.TakeDamage(slashDamageProfile, combatController.gameObject);
    //            }
    //            // If it's NOT a real enemy, THEN check if it's our special test dummy.
    //            else if (hit.CompareTag("TestingDummy") && hit.TryGetComponent<DamageableDummy>(out var dummyHealth))
    //            {
    //                Debug.Log("<color=red>FINISHER!</color> (on Test Dummy)");
    //                if (finisherDamageProfile != null && finisherDamageProfile.Count > 0)
    //                {
    //                    //combatController.GetComponent<DaggerAnimation>()?.PlayFinisherAnimation();
    //                    combatController.PlayerAnimationController.TriggerDagger_Finisher();
    //                    dummyHealth.TakeDamage(finisherDamageProfile[0].Value);
    //                }
    //            }
    //        }

    //    }
    //    else
    //    {
    //        swingSound.Play(combatController.transform);

    //        combatController.PlayerAnimationController.TriggerSecondaryAttack();

    //    }    
    //}
}