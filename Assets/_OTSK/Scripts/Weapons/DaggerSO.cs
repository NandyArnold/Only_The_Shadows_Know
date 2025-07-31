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
        // Later, this will check if the player is undetected for a finisher.
        Debug.Log($"Dagger: Attempting finisher for {finisherDamage} damage.");
    }
}