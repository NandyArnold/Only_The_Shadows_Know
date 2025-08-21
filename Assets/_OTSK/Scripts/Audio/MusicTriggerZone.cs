using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicTriggerZone : MonoBehaviour
{
    [Header("Category")]
    [Tooltip("Normal music will be muted during Endwalker state.")]
    public MusicCategory category = MusicCategory.Normal;

    [Header("Music Settings")]
    [Tooltip("The music track to play when the player enters this zone.")]
    [SerializeField] private AudioClip zoneMusic;
    [Tooltip("The target volume for this zone's music.")]
    [Range(0f, 1f)][SerializeField] private float zoneMusicVolume = 0.8f;

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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            _playerIsInside = true;

            // Tell the AudioManager to use this music as an override
            AudioManager.Instance.SetOverrideMusic(zoneMusic, zoneMusicVolume, category);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- NEW: Unset the flag ---
            _playerIsInside = false;

            // Tell the AudioManager to clear the override and go back to normal
            AudioManager.Instance.ClearOverrideMusic();
        }
    }
    private void HandleEndwalkerEnd()
    {
        // If Endwalker has just ended AND the player is still inside this zone...
        if (_playerIsInside)
        {
            // ...re-trigger the music.
            Debug.Log($"Endwalker ended. Re-activating music for {gameObject.name}");
            AudioManager.Instance.SetOverrideMusic(zoneMusic, zoneMusicVolume, category);
        }
    }
}