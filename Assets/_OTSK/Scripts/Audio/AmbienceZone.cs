using UnityEngine;
using DG.Tweening;


[RequireComponent(typeof(Collider))]
public class AmbienceZone : MonoBehaviour
{
    [Header("Category")]
    [Tooltip("Normal ambience will be muted during Endwalker state.")]
    public AmbienceCategory category = AmbienceCategory.Normal;

    [Header("Audio Settings")]
    [Tooltip("The ambient sound loop for this zone.")]
    [SerializeField] private AudioClip ambientClip;
    [Tooltip("The target volume for this sound when the player is inside the zone.")]
    [Range(0f, 1f)] public float targetVolume = 0.8f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 2.0f;

    private bool _playerIsInside = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnEnable()
    {
        
        PlayerController.OnEndwalkerStateEnded += HandleEndwalkerEnd;
    }

    private void OnDisable()
    {
        
        PlayerController.OnEndwalkerStateEnded -= HandleEndwalkerEnd;
    }

    // The trigger now just tells the zone to fade itself in or out
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerIsInside = true; // Set the flag
            AudioManager.Instance.EnterAmbienceZone(ambientClip, targetVolume, fadeInDuration, category);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerIsInside = false; // Unset the flag
            AudioManager.Instance.ExitAmbienceZone(ambientClip, fadeOutDuration);
        }
    }
    private void HandleEndwalkerEnd()
    {
        // If Endwalker has just ended AND the player is still inside this zone...
        if (_playerIsInside)
        {
            // ...re-trigger the ambient sound.
            Debug.Log($"Endwalker ended. Re-activating ambience for {gameObject.name}");
            AudioManager.Instance.EnterAmbienceZone(ambientClip, targetVolume, fadeInDuration, category);
        }
    }
}
