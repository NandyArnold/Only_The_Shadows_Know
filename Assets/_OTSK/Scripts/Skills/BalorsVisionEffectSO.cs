using UnityEngine;

[CreateAssetMenu(fileName = "FX_BalorsVision", menuName = "Only The Shadows Know/Skills/Effects/Balors Vision Effect")]
public class BalorsVisionEffectSO : SkillEffectSO
{
    [SerializeField] private float revealRadius = 15f;
    [SerializeField] private LayerMask revealLayerMask; // Set to everything you want to detect

    public override void Activate(GameObject caster)
    {
        Debug.Log("Activating Balor's Vision...");

        // Find all colliders within the reveal radius
        Collider[] hits = Physics.OverlapSphere(caster.transform.position, revealRadius, revealLayerMask);

        int revealedCount = 0;
        foreach (var hit in hits)
        {
            // Check if the object has the "HiddenObject" component
            if (hit.TryGetComponent<HiddenObject>(out var hiddenObject))
            {
                // If it does, tell the VFXManager to play the effect at its position
                VFXManager.Instance.PlayRevealEffect(hiddenObject.transform.position);
                revealedCount++;
            }
        }

        Debug.Log($"Balor's Vision revealed {revealedCount} hidden objects.");
    }
}