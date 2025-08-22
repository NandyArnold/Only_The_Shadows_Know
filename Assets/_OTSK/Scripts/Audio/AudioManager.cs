using DG.Tweening; // We'll use DOTween for smooth fading
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public struct AmbienceData
{
    public AmbienceCategory category;
    public float targetVolume;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA; // For the main music track (ambient or priority)
    [SerializeField] private AudioSource musicSourceB;
    //[SerializeField] private AudioSource musicSourceC;
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private List<AudioSource> ambienceSources;
    [SerializeField] private AudioSource channelSfxSource;
    [SerializeField] private AudioSource overrideAmbienceSource;

    private readonly Dictionary<AudioSource, AmbienceCategory> _activeAmbienceCategories = new Dictionary<AudioSource, AmbienceCategory>();
    private readonly Dictionary<AudioSource, AmbienceData> _activeAmbienceSounds = new Dictionary<AudioSource, AmbienceData>();

    private bool _isNormalAmbienceMuted = false;
    private bool _isNormalMusicMuted = false;

    private Tween _channelFadeTween;

    private Tween _overrideAmbienceFade;

    private MusicCategory _overrideMusicCategory;

    [Header("Priority Music")]
    [SerializeField] private AudioClip alertMusic;
    [SerializeField][Range(0f, 1f)] private float alertMusicVolume = 0.9f;
    [SerializeField] private AudioClip combatMusic;
    [SerializeField][Range(0f, 1f)] private float combatMusicVolume = 1f;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField][Range(0f, 1f)] private float gameOverMusicVolume = 1f;

    [Header("Audio Mixing")]
    [SerializeField] private AudioMixer gameMixer;
    [SerializeField] private AudioMixerSnapshot normalMix;
    [SerializeField] private AudioMixerSnapshot combatMix;
    [SerializeField] private float mixTransitionDuration = 1.0f;


    //private AudioClip _musicBeforeMute;
    //private float _volumeBeforeMute;

    private AudioSource _activeMusicSource;
    private ThreatState _currentThreatState = ThreatState.Safe;

    private AudioClip _currentSceneMusic; // The default track for the loaded level
    private float _currentSceneMusicVolume = 1f;

    private AudioClip _overrideMusic;
    private float _overrideMusicVolume = 1f;

    private Coroutine _activeMusicFade;
    private bool _isGameOver = false;
    private bool _isStoppingOverride = false;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicSourceA.loop = true;
        musicSourceB.loop = true;

        _activeMusicSource = musicSourceA;

    }

    public void PlayChanneledSound(AudioClip clip, float volume, float pitch, float fadeInDuration)
    {
        if (channelSfxSource == null || clip == null) return;

        // Kill any previous fade that might be running
        _channelFadeTween?.Kill();

        channelSfxSource.clip = clip;
        channelSfxSource.loop = true;
        channelSfxSource.pitch = pitch;

        // If there's a fade-in, start at 0 volume and fade up
        if (fadeInDuration > 0)
        {
            channelSfxSource.volume = 0f;
            _channelFadeTween = channelSfxSource.DOFade(volume, fadeInDuration);
        }
        else // Otherwise, just set the volume instantly
        {
            channelSfxSource.volume = volume;
        }

        channelSfxSource.Play();
    }

    // Call this to change the pitch, creating the "stretch" effect
    public void SetChanneledSoundPitch(float pitch)
    {
        if (channelSfxSource != null && channelSfxSource.isPlaying)
        {
            channelSfxSource.pitch = pitch;
        }
    }

    // Call this to stop the sound immediately
    public void StopChanneledSound(float fadeOutDuration)
    {
        if (channelSfxSource == null || !channelSfxSource.isPlaying) return;

        // Kill any previous fade
        _channelFadeTween?.Kill();

        // If there's a fade-out, fade to 0 and then stop
        if (fadeOutDuration > 0)
        {
            _channelFadeTween = channelSfxSource.DOFade(0f, fadeOutDuration)
                .OnComplete(() => {
                    channelSfxSource.Stop();
                    channelSfxSource.clip = null;
                });
        }
        else // Otherwise, just stop instantly
        {
            channelSfxSource.Stop();
            channelSfxSource.clip = null;
        }
    }

    // --- Music Control ---

    private void FadeBetweenMusic(AudioClip newClip, float volume, float duration)
    {
        if (_activeMusicFade != null) StopCoroutine(_activeMusicFade);
        _activeMusicFade = StartCoroutine(FadeBetweenMusicRoutine(newClip, volume, duration));
    }


    private IEnumerator FadeBetweenMusicRoutine(AudioClip newClip, float targetVolume, float duration)
    {
        AudioSource activeSource = _activeMusicSource;
        AudioSource newSource = (_activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;

        newSource.clip = newClip;
        newSource.volume = 0;
        newSource.loop = true;
        newSource.Play();

        if (activeSource.isPlaying) activeSource.DOFade(0f, duration);
        yield return newSource.DOFade(targetVolume, duration).WaitForCompletion();

        activeSource.Stop();
        activeSource.clip = null;
        _activeMusicSource = newSource;
        _activeMusicFade = null;
    }


    // --- UI Sound Control ---

    public void PlayUISound(AudioClip uiClip)
    {
        if (uiClip == null) return;
        uiSfxSource.PlayOneShot(uiClip);
    }

 

    // Called by MusicTriggerZone
    public void SetSceneMusic(AudioClip sceneMusic, float volume)
    {
        _currentSceneMusic = sceneMusic;
        _currentSceneMusicVolume = volume;
        UpdateMusic();
    }

    public void SetOverrideMusic(AudioClip overrideClip, float volume, MusicCategory category)
    {
        // --- THE GATEKEEPER LOGIC ---
        // If normal music is muted AND this is a Normal category track, do nothing.
        if (_isNormalMusicMuted && category == MusicCategory.Normal)
        {
            return; // Ignore the request
        }

        _overrideMusic = overrideClip;
        _overrideMusicVolume = volume;
        UpdateMusic();
    }

    public void ClearOverrideMusic()
    {
        _overrideMusic = null;
        UpdateMusic();
    }
    public void MuteNormalMusic(bool isMuted, float fadeDuration)
    {
        // Don't change anything if the state is the same
        if (_isNormalMusicMuted == isMuted) return;

        _isNormalMusicMuted = isMuted;

        // Just re-evaluate the music state. UpdateMusic() will handle the rest.
        UpdateMusic();
    }
    public void RestoreNormalMusic(float fadeDuration)
    {
        // This method is now identical to MuteNormalMusic(false, ...).
        // We keep it for semantic clarity in the PlayerController.
        MuteNormalMusic(false, fadeDuration);
    }
    public void SetThreatState(ThreatState newState)
    {
        if (_currentThreatState == newState) return;
        Debug.Log($"<color=lime>DEBUG (AUDIO):</color> SetThreatState called with new state: {newState}.");
        _currentThreatState = newState;
        UpdateMusic();
        if (newState == ThreatState.Safe)
        {
            // Transition to the normal mix
            normalMix.TransitionTo(mixTransitionDuration);
            //FadeInAllAmbience();
        }
        else
        {
            //if (overrideAmbienceSource.isPlaying)
            //{
            //    Debug.Log("Threat state changed to Alert/Combat, stopping Endwalker override ambience.");
            //    StopOverrideAmbience(0.5f); // Use a quick 0.5s fade out
            //}
            //Transition to the combat mix
            combatMix.TransitionTo(mixTransitionDuration);
            FadeOutAllAmbience();
        }
    }
    private void UpdateMusic()
    {
        if (_isStoppingOverride) return;

        AudioClip targetClip = null;
        float targetVolume = 1f;

        MusicCategory targetCategory = MusicCategory.Normal;

        // 1. Determine the highest-priority music that SHOULD be playing.
        if (_isGameOver) { targetClip = gameOverMusic; targetVolume = gameOverMusicVolume; }
        else if (_currentThreatState == ThreatState.Combat) { targetClip = combatMusic; targetVolume = combatMusicVolume; }
        else if (_currentThreatState == ThreatState.Alert) { targetClip = alertMusic; targetVolume = alertMusicVolume; }
        else if (_overrideMusic != null)
        {
            targetClip = _overrideMusic;
            targetVolume = _overrideMusicVolume;
            // We need to fetch the category from a variable that MusicTriggerZone sets.
            // Assuming you have _overrideMusicCategory as a class member:
            // targetCategory = _overrideMusicCategory;
        }
        else { targetClip = _currentSceneMusic; targetVolume = _currentSceneMusicVolume; }

        // 2. THE NEW GATEKEEPER: Check if Endwalker should mute this track.
        if (_isNormalMusicMuted && targetCategory == MusicCategory.Normal)
        {
            // If the chosen music is "Normal" but we're in Endwalker, force silence.
            targetClip = null;
        }

        // 3. Apply the final decision.
        if (_activeMusicSource.clip == targetClip)
        {
            // If the correct clip is already playing, just ensure the volume is correct.
            _activeMusicSource.DOFade(targetVolume, 0.5f);
            return;
        }


        if (targetClip != null)
        {
            // If a new clip needs to be played, fade to it.
            FadeBetweenMusic(targetClip, targetVolume, 1.0f);
        }
        else
        {
            // If no clip should be playing, fade the current music out.
            _activeMusicSource.DOFade(0f, 1.0f);
        }
    }


    public void EnterAmbienceZone(AudioClip clip, float volume, float fadeDuration, AmbienceCategory category)
    {
        // If normal ambience is muted AND this is a Normal category zone, do nothing.
        if (_isNormalAmbienceMuted && category == AmbienceCategory.Normal)
        {
            return; // Ignore the request to start this ambient sound
        }
        // First, check if this sound is already playing to avoid duplicates
        foreach (var source in ambienceSources)
        {
            if (source.clip == clip && source.isPlaying)
            {
                // If it's already playing but faded out, just fade it back in
                source.DOKill();
                source.DOFade(volume, fadeDuration);
                return;
            }
        }

        AudioSource availableSource = GetAvailableAmbienceSource();
        if (availableSource != null)
        {
            _activeAmbienceSounds[availableSource] = new AmbienceData { category = category, targetVolume = volume };

            availableSource.clip = clip;
            availableSource.volume = 0;
            availableSource.loop = true;
            availableSource.Play();

            if (_currentThreatState == ThreatState.Safe)
            {
                availableSource.DOKill();
                availableSource.DOFade(volume, fadeDuration);
            }
        }
    }

    public void ExitAmbienceZone(AudioClip clip, float fadeDuration)
    {
        foreach (var source in ambienceSources)
        {
            if (source.clip == clip)
            {
                source.DOKill();
                source.DOFade(0, fadeDuration).OnComplete(() => {
                    source.Stop();
                    source.clip = null; // Frees up the source for the pool
                                        // --- REMOVE FROM DICTIONARY ---
                    _activeAmbienceSounds.Remove(source);
                });
                return;
            }
        }
    }

    public void MuteNormalAmbience(float fadeDuration)
    {
        
        // We now iterate through the correct dictionary that holds all active sounds.
        foreach (var activeSound in _activeAmbienceSounds)
        {
            // Check the 'category' from the AmbienceData struct
            if (activeSound.Value.category == AmbienceCategory.Normal)
            {
                // Fade out the AudioSource
                activeSound.Key.DOFade(0, fadeDuration);
            }
        }
    }
    public void RestoreNormalAmbience(float fadeDuration)
    {
        // Now that we're leaving Endwalker, unmute normal ambience
        _isNormalAmbienceMuted = false;

        // Check all active sounds
        foreach (var activeSound in _activeAmbienceSounds)
        {
            // If we find one that is "Normal"...
            if (activeSound.Value.category == AmbienceCategory.Normal)
            {
                // ...fade it back in to its original target volume.
                activeSound.Key.DOFade(activeSound.Value.targetVolume, fadeDuration);
            }
        }
    }

    private AudioSource GetAvailableAmbienceSource()
    {
        foreach (var source in ambienceSources)
        {
            // A source is available if it has no clip assigned to it.
            if (source.clip == null)
            {
                return source;
            }
        }
        Debug.LogWarning("No available ambience sources!");
        return null;
    }


    private void FadeOutAllAmbience(float duration = 0.5f)
    {
        // This now correctly iterates through the source pool
        foreach (var source in ambienceSources)
        {
            if (source.isPlaying)
            {
                source.DOKill();
                source.DOFade(0, duration);
            }
        }
    }
    private void FadeInAllAmbience(float duration = 1.0f)
    {
        // This method is no longer needed in the new system, as zones are
        // faded in individually by EnterAmbienceZone. 
        // We leave it empty to prevent errors.
    }



    public void StopMusic(float fadeDuration = 0.5f)
    {
        _activeMusicSource.DOFade(0f, fadeDuration);
    }
    public void ResetAudioState()
    {
        _currentThreatState = ThreatState.Safe;
        _overrideMusic = null;
        // You could also stop all ambience here if needed
    }



    // This is called when the GameOverScreen is hidden.
    public void StopGameOverMusic()
    {
        // This triggers the normal priority check, which will figure out
        // if it should go back to scene music, combat music, etc.
        UpdateMusic();
    }
    public void SetGameOver(bool isGameOver)
    {
        if (_isGameOver == isGameOver) return;
        _isGameOver = isGameOver;
        UpdateMusic();
        if (isGameOver) FadeOutAllAmbience();
    }

    public bool IsSafe()
    {
        return _currentThreatState == ThreatState.Safe && !_isGameOver;
    }

    public void PlayOverrideAmbience(AudioClip clip, float volume, float fadeIn)
    {
        if (overrideAmbienceSource == null || clip == null) return;


        _isNormalAmbienceMuted = true;
        MuteNormalAmbience(fadeIn);

        overrideAmbienceSource.clip = clip;
        overrideAmbienceSource.loop = true;
        overrideAmbienceSource.volume = 0;
        overrideAmbienceSource.Play();
        overrideAmbienceSource.DOFade(volume, fadeIn);
    }

    public void StopOverrideAmbience(float fadeOut)
    {
        if (overrideAmbienceSource == null) return;

        Debug.Log("<color=yellow>DEBUG 2 (AUDIO):</color> StopOverrideAmbience has been called.");
        
        _overrideAmbienceFade?.Kill();

        _isStoppingOverride = true;
        Debug.Log("<color=yellow>DEBUG 3 (AUDIO):</color> _isStoppingOverride flag SET to TRUE.");

        _isNormalAmbienceMuted = false;

        // Fade out the Endwalker ambience
        _overrideAmbienceFade = overrideAmbienceSource.DOFade(0, fadeOut)
         .OnComplete(() => {
             overrideAmbienceSource.Stop();
             // --- SET THE FLAG to false when we finish ---
             _isStoppingOverride = false;
             Debug.Log("<color=yellow>DEBUG 5 (AUDIO):</color> Endwalker fade-out COMPLETE. _isStoppingOverride flag SET to FALSE.");
            UpdateMusic();
         });

         RestoreNormalAmbience(fadeOut);

    }
}