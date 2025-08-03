// BowSO.cs - UPGRADED with Spread and Focused Shot
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_Bow", menuName = "Only The Shadows Know/Weapons/Bow")]
public class BowSO : WeaponSO
{
    [Header("Bow Specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [Tooltip("Which layers the aiming raycast will hit.")]
    [SerializeField] private LayerMask aimLayerMask;
    [Tooltip("The max radius (in screen pixels) for the random spread of an unfocused shot.")]
    [SerializeField] private float unfocusedSpreadRadius = 25f;

    public void Fire(PlayerCombat combatController, bool isFocused)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 aimPoint = screenCenter;

        // If the shot is NOT focused, add a random spread.
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
            Instantiate(arrowPrefab, combatController.FirePoint.position, arrowRotation);
        }
    }

    // Unfocused shot (LMB)
    public override void PrimaryAttack(PlayerCombat combatController)
    {
        Fire(combatController, false);
    }

    // Focused shot (RMB)
    public override void SecondaryAttack(PlayerCombat combatController)
    {
        Fire(combatController, true);
    }
}