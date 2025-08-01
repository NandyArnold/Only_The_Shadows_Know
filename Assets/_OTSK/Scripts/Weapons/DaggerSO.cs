// DaggerSO.cs - UPGRADED

using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Dagger", menuName = "Only The Shadows Know/Weapons/Dagger")]
public class DaggerSO : WeaponSO
{
    [Header("Dagger Attack Settings")]
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Dagger Damage")]
    [SerializeField] private float slashDamage = 10f;
    [SerializeField] private float finisherDamage = 100f;

    // The Primary Attack uses the LEFT hand.
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        // 1. Tell the animator to play the attack.
        combatController.PlayerAnimationController.TriggerPrimaryAttack();

        // 2. Perform the hit detection.
        Collider[] hits = Physics.OverlapSphere(combatController.HandSocketL.position, attackRange, hittableLayers);
        if (hits.Any())
        {
            foreach (var hit in hits)
            {
                Debug.Log($"<color=green>Dagger (R):</color> Hit {hit.gameObject.name} for {slashDamage} damage.");
                // TODO: Call a TakeDamage() method on the hit object.
            }
        }
    }

    // The Secondary Attack uses the RIGHT hand.
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        // 1. Tell the animator to play the attack.
        combatController.PlayerAnimationController.TriggerSecondaryAttack();

        // 2. Perform the hit detection.
        bool canPerformFinisher = false; // Placeholder for finisher logic
        if (canPerformFinisher)
        {
            // Finisher logic...
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(combatController.HandSocketR.position, attackRange, hittableLayers);
            if (hits.Any())
            {
                foreach (var hit in hits)
                {
                    Debug.Log($"<color=cyan>Dagger (L):</color> Hit {hit.gameObject.name} for {slashDamage} damage.");
                    // TODO: Call a TakeDamage() method on the hit object.
                }
            }
        }
    }
}