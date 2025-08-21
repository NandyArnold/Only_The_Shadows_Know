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

        // Stop the loop-starting coroutine if it's running.
        if (_loopCoroutine != null)
        {
            StopCoroutine(_loopCoroutine);
            _loopCoroutine = null;
        }

        // Tell the AudioManager to stop the looping sound.
        AudioManager.Instance.StopChanneledSound(_currentProfile.loopFadeOutDuration);

        // Play the final one-shot sound.
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
}