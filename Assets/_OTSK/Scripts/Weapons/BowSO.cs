using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Bow", menuName = "Only The Shadows Know/Weapons/Bow")]
public class BowSO : WeaponSO
{
    [Header("Bow Specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private float unfocusedSpreadRadius = 25f;

    // NOTE: The speed and fire rate variables are no longer here.

    public void Fire(PlayerCombat combatController, bool isFocused)
    {
        // Fire rate and speed are now handled by the projectile itself or player stats.
        // We will re-add the fire rate limiter to PlayerCombat later.

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 aimPoint = screenCenter;

        if (!isFocused)
        {
            aimPoint += Random.insideUnitCircle * unfocusedSpreadRadius;
        }

        Ray ray = mainCamera.ScreenPointToRay(aimPoint);
        Vector3 targetPoint;
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

            // Get the projectile script from the new arrow.
            if (arrowObject.TryGetComponent<ArrowProjectile>(out var projectile))
            {
                // Tell the projectile whether the shot was focused or not.
                projectile.Initialize(isFocused);
            }
        }
    }

    public override void PrimaryAttack(PlayerCombat combatController)
    {
        Fire(combatController, false);
    }

    public override void SecondaryAttack(PlayerCombat combatController)
    {
        Fire(combatController, true);
    }
}