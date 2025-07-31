// DaggerSO.cs

using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Dagger", menuName = "Only The Shadows Know/Weapons/Dagger")]
public class DaggerSO : WeaponSO
{
    [Header("Dagger Specifics")]
    [SerializeField] private float slashDamage = 10f;
    [SerializeField] private float finisherDamage = 100f;

    public override void PrimaryAttack(PlayerCombat combatController)
    {
        // For now, we'll just log to the console to confirm it works.
        // Later, this will trigger an animation and a damage collider.
        Debug.Log($"Dagger: Performing quick slash for {slashDamage} damage.");
    }

    public override void SecondaryAttack(PlayerCombat combatController)
    {
        bool canPerformFinisher = false;

        if (canPerformFinisher)
        {
            Debug.Log($"Dagger: Performing FINISHER for {finisherDamage} damage.");
            // Finisher logic will go here
        }
        else
        {
            // This is your new idea: perform a different slash using the primary attack's damage.
            Debug.Log($"Dagger: Performing alternate heavy slash for {slashDamage} damage.");
            // The actual damage dealing logic will go here later.
        }
    }
}