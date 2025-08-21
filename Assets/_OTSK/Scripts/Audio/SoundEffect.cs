// SoundEffect.cs
using UnityEngine;
using System.Collections;

// This ensures the GameObject will always have an AudioSource component.
[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _returnCoroutine;

    private void Awake()
    {
        // Get the AudioSource component on this GameObject.
        _audioSource = GetComponent<AudioSource>();
    }

    // This is the main method called by the SoundEffectManager.
    public void Play(AudioClip clip, float volume, float pitch)
    {
        // Configure the AudioSource with the provided settings.
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.pitch = pitch;

        // Play the sound.
        _audioSource.Play();

        // If a previous "return to pool" coroutine was running, stop it.
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
        }

        // Start a new coroutine to return this object to the pool after the clip finishes.
        _returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay(clip.length));
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        // Wait for the duration of the audio clip.
        yield return new WaitForSeconds(delay);

        // Tell the manager to take this object back.
        SoundEffectManager.Instance.ReturnToPool(this);
    }
}