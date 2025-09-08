using UnityEngine;

/// <summary>
/// A trigger volume that can temporarily lock and re-enable a specific ability
/// while the player is inside it.
/// </summary>
public class AbilityControlZone : MonoBehaviour
{
    [Tooltip("The ability to lock. Drag any UnlockableAbilitySO, WeaponSO, or SkillSO asset here.")]
    [SerializeField] private ScriptableObject abilityToControl;

    [Tooltip("Optional: A list of additional abilities to control with this zone.")]
    [SerializeField] private ScriptableObject[] additionalAbilitiesToControl;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (abilityToControl != null)
            {
                AbilityUnlockManager.Instance.TemporarilyLock(abilityToControl);
                Debug.Log($"Player entered zone. Locking {abilityToControl.name}.");
            }
            foreach (var ability in additionalAbilitiesToControl)
            {
                AbilityUnlockManager.Instance.TemporarilyLock(ability);
                Debug.Log($"Player entered zone. Locking {ability.name}.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (abilityToControl != null)
            {
                AbilityUnlockManager.Instance.ReEnableAbility(abilityToControl);
                Debug.Log($"Player exited zone. Re-enabling {abilityToControl.name}.");
            }
            foreach (var ability in additionalAbilitiesToControl)
            {
                AbilityUnlockManager.Instance.ReEnableAbility(ability);
                Debug.Log($"Player exited zone. Re-enabling {ability.name}.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a semi-transparent box in the editor to visualize the trigger area
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
