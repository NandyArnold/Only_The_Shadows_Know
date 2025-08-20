using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Collider))]
public class AmbienceZone : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The ambient sound loop for this zone.")]
    [SerializeField] private AudioClip ambientClip;
    [Tooltip("The target volume for this sound when the player is inside the zone.")]
    [Range(0f, 1f)] public float targetVolume = 0.8f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 2.0f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

  
    // The trigger now just tells the zone to fade itself in or out
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
