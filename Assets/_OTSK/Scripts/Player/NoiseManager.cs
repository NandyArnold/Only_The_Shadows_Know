using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance { get; private set; }

    [Header("Sound Occlusion")]
    [Tooltip("Layers that can block the direct line of sound (e.g., Walls, Default).")]
    [SerializeField] private LayerMask occlusionLayers;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
      
    }

    // This method is now much more intelligent.
    public void GenerateNoise(Vector3 position, float intensity, GameObject sourceObject = null)
    {
        if (EnemyManager.Instance == null) return;
        if (sourceObject != null && sourceObject.CompareTag("Player"))
        {
            if (GameManager.Instance.Player != null && GameManager.Instance.Player.IsInEndwalkerState)
            {
                return; // Player is invisible and silent, so no noise is generated.
            }
        }

        var activeEnemies = EnemyManager.Instance.GetActiveEnemies();

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || enemy.gameObject == sourceObject) continue;

            float distance = Vector3.Distance(enemy.transform.position, position);
            if (distance > enemy.Config.hearingRange) continue;

            float finalIntensity = intensity;

            // --- 1. OCCLUSION CHECK (Walls) ---
            if (Physics.Linecast(position, enemy.Detector.EyePoint.position, out RaycastHit hit, occlusionLayers))
            {
                // If the line of sound is blocked by an occluding object...
                if (hit.collider.TryGetComponent<NoiseDampeningZone>(out var zone) && zone.type == NoiseDampeningZone.ZoneType.Occlusion)
                {
                    // ...apply that zone's dampening.
                    finalIntensity *= zone.dampeningMultiplier;
                }
            }

            // --- 2. AREA CHECK (Muffled Zones) ---
            Collider[] listenerColliders = Physics.OverlapSphere(enemy.transform.position, 0.1f);
            foreach (var col in listenerColliders)
            {
                // If the LISTENER is inside an area-based dampening zone...
                if (col.TryGetComponent<NoiseDampeningZone>(out var zone) && zone.type == NoiseDampeningZone.ZoneType.Area)
                {
                    // ...apply that zone's dampening.
                    finalIntensity *= zone.dampeningMultiplier;
                    break; // Only apply one area effect
                }
            }

            // Tell the enemy's detector about the final, modified sound intensity.
            if (finalIntensity > 0)
            {
                enemy.Detector.OnSoundHeard(position, finalIntensity);
            }
        }
    }
}