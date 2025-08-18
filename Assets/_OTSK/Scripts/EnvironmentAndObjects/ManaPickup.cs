// Create this new script, ManaPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ManaPickup : MonoBehaviour
{
    [SerializeField] private float manaToRestore = 25f;
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSFX;

    private void Awake()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            // Try to get the PlayerStats component
            if (other.TryGetComponent<PlayerStats>(out var playerStats))
            {
                // Restore mana
                playerStats.RestoreMana(manaToRestore);

                // Play effects
                if (pickupVFX != null) Instantiate(pickupVFX, transform.position, Quaternion.identity);
                if (pickupSFX != null) SoundEffectManager.Instance.PlaySoundAtPoint(pickupSFX, transform.position);

                // Destroy the pickup object
                Destroy(gameObject);
            }
        }
    }
}