using UnityEngine;

public class ObjectiveReward : MonoBehaviour
{
    [Tooltip("Drag any UnlockableAbilitySO, WeaponSO, or SkillSO asset here in the Inspector.")]
    [SerializeField] private ScriptableObject abilityToUnlock;

    [Tooltip("Should this trigger deactivate itself after granting the reward once?")]
    [SerializeField] private bool fireOnce = true;

    /// <summary>
    /// This is a built-in Unity function that is automatically called
    /// when another collider enters the trigger zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // First, we check if the object that entered has the "Player" tag.
        if (other.CompareTag("Player"))
        {
            // If it is the player, we call our GrantReward method.
            GrantReward();
        }
    }

    /// <summary>
    /// This method contains the core logic for granting the ability.
    /// </summary>
    public void GrantReward()
    {
        if (abilityToUnlock != null)
        {
            Debug.Log($"<color=lime>Player triggered reward!</color> Unlocking: {abilityToUnlock.name}");
            AbilityUnlockManager.Instance.GrantAbility(abilityToUnlock);

            // If this is a one-shot trigger, deactivate the GameObject so it can't be triggered again.
            if (fireOnce)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a green box in the editor to visualize the trigger area
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}