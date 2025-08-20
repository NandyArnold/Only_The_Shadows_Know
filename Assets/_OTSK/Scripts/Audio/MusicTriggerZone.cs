using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicTriggerZone : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("The music track to play when the player enters this zone.")]
    [SerializeField] private AudioClip zoneMusic;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the AudioManager to use this music as an override
            AudioManager.Instance.SetOverrideMusic(zoneMusic);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the AudioManager to clear the override and go back to normal
            AudioManager.Instance.ClearOverrideMusic();
        }
    }
}