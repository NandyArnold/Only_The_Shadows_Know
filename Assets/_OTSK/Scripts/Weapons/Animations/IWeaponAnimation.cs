// IWeaponAnimation.cs

public interface IWeaponAnimation
{
    void PlayPrimaryAttack();
    void PlaySecondaryAttack();
    void SetAiming(bool isAiming);
    // We can add more methods like SetAiming(bool isAiming) here later for the bow.
}
