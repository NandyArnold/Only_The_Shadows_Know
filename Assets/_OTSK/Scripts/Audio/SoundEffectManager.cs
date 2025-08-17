// Create this new script, SoundEffectManager.cs
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }

    public void PlayInstaKillSound()
        {
        AudioClip instaKillClip = Resources.Load<AudioClip>("Audio/InstaKillSound");
        if (instaKillClip != null)
        {
            PlaySound(instaKillClip, 1f);
        }
        else
        {
            Debug.LogWarning("InstaKill sound clip not found!");
        }
    }

}
