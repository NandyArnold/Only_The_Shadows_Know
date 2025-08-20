using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbienceZone : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip ambientClip;
    [Range(0f, 1f)]
    [SerializeField] private float targetVolume = 0.8f;
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 2.0f;

    // No AudioSource variable or Awake method is needed here anymore.

    private void Awake()
    {
        // Still need to ensure the collider is a trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.EnterAmbienceZone(ambientClip, targetVolume, fadeInDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.ExitAmbienceZone(ambientClip, fadeOutDuration);
        }
    }
}