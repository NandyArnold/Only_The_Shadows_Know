using UnityEngine;


[CreateAssetMenu(fileName = "Weapon_Bow", menuName = "Only The Shadows Know/Weapons/Bow")]
public class BowSO : WeaponSO
{
    [Header("Bow Specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private LayerMask aimLayerMask;

    [Header("Combat Stats")] // Added a new header for organization
    [SerializeField] private float timeBetweenShots = 0.5f;
    [SerializeField] private float unfocusedSpreadRadius = 25f;

   

    // Static variable to track the last fire time for this weapon type.
    private static float _lastFireTime;

    public void Fire(PlayerCombat combatController, bool isFocused)
    {
        // Add the cooldown check at the very top.
        if (Time.time < _lastFireTime + timeBetweenShots)
        {
            return;
        }
        _lastFireTime = Time.time;
      
        Debug.Log("1. BowSO Fire method called. Attempting to trigger animation...");
        combatController.PlayerAnimationController.TriggerPrimaryAttack();


        // ... (The rest of the Fire method is unchanged)
        
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