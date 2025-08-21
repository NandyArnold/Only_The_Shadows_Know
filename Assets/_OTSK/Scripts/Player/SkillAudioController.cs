// SkillAudioController.cs
using System.Collections;
using UnityEngine;

public class SkillAudioController : MonoBehaviour
{
    private SkillAudioProfile _currentProfile;
    private Transform _currentEmitter;
    private Coroutine _loopCoroutine;

    // Called by a skill to start its audio sequence.
    public void Play(SkillAudioProfile profile, Transform emitter)
    {
        // Stop any previous skill sound that might be playing.
        Stop();

        if (profile == null) return;

        _currentProfile = profile;
        _currentEmitter = emitter;

        // Play the initial one-shot sound immediately.
        _currentProfile.castStartSound.Play(_currentEmitter);

        // If there's a loop, start the coroutine to play it after a delay.
        if (_currentProfile.channelLoopSound != null)
        {
            _loopCoroutine = StartCoroutine(StartLoopAfterDelay());
        }
    }

    // Called by a skill when it stops channeling.
    public void Stop()
    {
        if (_currentProfile == null) return;

        StopLoopOnly(); // Call the new method to stop the loop

        // Then, play the final one-shot sound
        _currentProfile.castEndSound.Play(_currentEmitter);

        _currentProfile = null;
    }

    private IEnumerator StartLoopAfterDelay()
    {
        // Wait for the specified delay.
        yield return new WaitForSeconds(_currentProfile.loopStartDelay);

        // Tell the AudioManager to start the looping sound.
        AudioManager.Instance.PlayChanneledSound(
         _currentProfile.channelLoopSound,
         _currentProfile.channelVolume,
         _currentProfile.channelPitch,
         _currentProfile.loopFadeInDuration
     );
    }
    public void StopLoopOnly()
    {
        if (_currentProfile == null) return;

        if (_loopCoroutine != null)
        {
            StopCoroutine(_loopCoroutine);
            _loopCoroutine = null;
        }

        // Only stop the looping sound, do NOT play the end sound
        AudioManager.Instance.StopChanneledSound(_currentProfile.loopFadeOutDuration);
    }
}