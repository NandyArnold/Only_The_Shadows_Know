using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Bow", menuName = "Only The Shadows Know/Weapons/Bow")]
public class BowSO : WeaponSO
{
    [Header("Bow Specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private LayerMask aimLayerMask;

    [Header("Combat Stats")]
    [SerializeField] private float arrowSpeed = 30f;
    [SerializeField] private float focusedArrowSpeed = 50f;
    [SerializeField] private float unfocusedSpreadRadius = 25f;

    [Header("Damage Profiles")]
    public List<DamageInstance> unfocusedDamageProfile;
    public List<DamageInstance> focusedDamageProfile;


    // A central fire method to avoid repeating code
    private void Fire(PlayerCombat combatController, bool isFocused)
    {
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 aimPoint = screenCenter;
        if (!isFocused)
        {
            aimPoint += UnityEngine.Random.insideUnitCircle * unfocusedSpreadRadius;
        }

        Ray ray = mainCamera.ScreenPointToRay(aimPoint);
        Vector3 targetPoint;
        //Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - combatController.FirePoint.position).normalized;
        Quaternion arrowRotation = Quaternion.LookRotation(direction);

        if (arrowPrefab != null)
        {
            
            GameObject arrowObject = Instantiate(arrowPrefab, combatController.FirePoint.position, arrowRotation);
            if (arrowObject.TryGetComponent<ArrowProjectile>(out var projectile))
            {
                // Determine which speed and damage profile to use
                float currentSpeed = isFocused ? focusedArrowSpeed : arrowSpeed;
                List<DamageInstance> damageToApply = isFocused ? focusedDamageProfile : unfocusedDamageProfile;

                // Initialize the projectile with all the correct data
                projectile.Initialize(combatController.gameObject, damageToApply, currentSpeed);
        combatController.PlayerAnimationController.TriggerPrimaryAttack();
            }
        }
    }

    public override void PrimaryAttack(PlayerCombat combatController)
    {
        // For the bow, LMB press depends on the focus state. PlayerCombat handles this.
        // We will call the appropriate Fire method from PlayerCombat.
        // This is a placeholder, the real logic is in PlayerCombat's HandlePrimaryAttack.
        bool isFocused = combatController.IsFocused;
        Fire(combatController, isFocused);
    }

    public override void SecondaryAttack(PlayerCombat combatController)
    {
        // For the bow, RMB is for focusing, not attacking.
        // The actual "focused shot" is triggered by PrimaryAttack while focused.
        // This method can be left empty, as PlayerCombat handles the focus state.
        bool isFocused = combatController.IsFocused;
        Fire(combatController, isFocused);
    }
}