// SoundEffect.cs (Manual 3D Volume Version)
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    private AudioSource _audioSource;
    private Transform _listener;
    private float _initialVolume;
    private float _maxDistance;
    private Coroutine _returnCoroutine; // Your robust coroutine handling

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (Camera.main != null)
        {
            _listener = Camera.main.transform;
        }
    }

    private void Update()
    {
        // If we have a listener and the sound is playing, manually adjust volume
        if (_listener != null && _audioSource.isPlaying)
        {
            float distance = Vector3.Distance(transform.position, _listener.position);

            // This creates a logarithmic-like falloff (inverse square curve)
            float normalizedDistance = Mathf.Clamp01(distance / _maxDistance);
            float volumeMultiplier = 1.0f - (normalizedDistance * normalizedDistance);

            _audioSource.volume = _initialVolume * volumeMultiplier;
        }
    }

    public void Play(AudioClip clip, float volume, float pitch)
    {
        _initialVolume = volume;
        _maxDistance = _audioSource.maxDistance > 0 ? _audioSource.maxDistance : 50f;

        if (_listener == null)
        {
            if (Camera.main != null) _listener = Camera.main.transform;
            else { Debug.LogError("No Audio Listener (Main Camera) found in scene!"); return; }
        }

        _audioSource.clip = clip;
        _audioSource.pitch = pitch;
        _audioSource.Play();

        // Your existing robust coroutine handling is preserved
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
        }
        _returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay(clip.length / pitch)); // Dividing by pitch for accuracy
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundEffectManager.Instance.ReturnToPool(this);
    }
}