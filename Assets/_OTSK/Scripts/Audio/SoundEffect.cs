using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // This is the main method the manager will call to play a sound.
    public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.pitch = pitch;
        _audioSource.Play();

        // Start a coroutine to automatically return this object to the pool.
        StartCoroutine(ReturnToPoolAfterPlay());
    }

    private IEnumerator ReturnToPoolAfterPlay()
    {
        // Wait for the length of the audio clip.
        yield return new WaitForSeconds(_audioSource.clip.length);

        // Tell the manager that this object is now available.
        SoundEffectManager.Instance.ReturnToPool(this);
    }
}