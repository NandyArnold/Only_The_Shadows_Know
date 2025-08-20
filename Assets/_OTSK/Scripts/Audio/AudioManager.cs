using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // We'll use DOTween for smooth fading



public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // For the main music track (ambient or priority)
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private List<AudioSource> ambienceSources;

    [Header("Priority Music")]
    [SerializeField] private AudioClip alertMusic;
    [SerializeField] private AudioClip combatMusic;

    private ThreatState _currentThreatState = ThreatState.Safe;
    private AudioClip _currentSceneMusic; // The default track for the loaded level
    private AudioClip _overrideMusic;
    private Coroutine _activeMusicFade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    
    }

    // --- Music Control ---

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicClip == null) return;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void FadeInMusic(AudioClip musicClip, float duration = 1.0f, bool loop = true)
    {
        if (musicClip == null) return;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.volume = 0;
        musicSource.Play();
        musicSource.DOFade(1f, duration);
    }

    private void FadeBetweenMusic(AudioClip newClip, float duration = 1.0f)
    {
        if (_activeMusicFade != null) StopCoroutine(_activeMusicFade);
        _activeMusicFade = StartCoroutine(FadeBetweenMusicRoutine(newClip, duration));
    }


    private IEnumerator FadeBetweenMusicRoutine(AudioClip newClip, float duration)
    {
        if (musicSource.isPlaying)
        {
            yield return musicSource.DOFade(0f, duration / 2).WaitForCompletion();
        }

        musicSource.clip = newClip;
        musicSource.Play();
        yield return musicSource.DOFade(1f, duration / 2).WaitForCompletion();
    }
    

    // --- UI Sound Control ---

    public void PlayUISound(AudioClip uiClip)
    {
        if (uiClip == null) return;
        uiSfxSource.PlayOneShot(uiClip);
    }

    public void SetSceneMusic(AudioClip sceneMusic)
    {
        _currentSceneMusic = sceneMusic;
        UpdateMusic();
    }

    // Called by MusicTriggerZone
    public void SetOverrideMusic(AudioClip overrideClip)
    {
        _overrideMusic = overrideClip;
        UpdateMusic();
    }

    public void ClearOverrideMusic()
    {
        _overrideMusic = null;
        UpdateMusic();
    }
    public void SetThreatState(ThreatState newState)
    {
        if (_currentThreatState == newState) return;
        _currentThreatState = newState;
        UpdateMusic();

        // Also control the ambience layers
        if (newState == ThreatState.Safe)
        {
            FadeInAllAmbience();
        }
        else
        {
            FadeOutAllAmbience();
        }
    }
    private void UpdateMusic()
    {
        AudioClip targetClip = null;

        // Priority 1: Combat
        if (_currentThreatState == ThreatState.Combat)
        {
            targetClip = combatMusic;
        }
        // Priority 2: Alert
        else if (_currentThreatState == ThreatState.Alert)
        {
            targetClip = alertMusic;
        }
        // Priority 3: Music Zone Override
        else if (_overrideMusic != null)
        {
            targetClip = _overrideMusic;
        }
        // Priority 4: Default Scene Music
        else
        {
            targetClip = _currentSceneMusic;
        }

        // Only fade if the target clip is different from what's currently playing
        if (targetClip != null && musicSource.clip != targetClip)
        {
            FadeBetweenMusic(targetClip);
        }
    }


    public void EnterAmbienceZone(AudioClip clip, float volume, float fadeDuration)
    {
        AudioSource source = GetAvailableAmbienceSource();
        if (source != null)
        {
            source.clip = clip;
            source.Play();
            if (_currentThreatState == ThreatState.Safe) // Only fade in if we are not in combat
            {
                source.DOFade(volume, fadeDuration);
            }
        }
    }

    public void ExitAmbienceZone(AudioClip clip, float fadeDuration)
    {
        foreach (var source in ambienceSources)
        {
            if (source.clip == clip)
            {
                source.DOFade(0, fadeDuration).OnComplete(() => {
                    source.Stop();
                    source.clip = null;
                });
                return;
            }
        }
    }

    private void PlayMusicTrack(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        if (_activeMusicFade != null) StopCoroutine(_activeMusicFade);
        _activeMusicFade = StartCoroutine(FadeBetweenMusicRoutine(clip, 1.0f));
    }

    public void PlayMusicOverride(AudioClip overrideClip)
    {
        if (musicSource.clip == overrideClip) return; // Don't restart if already playing
        FadeBetweenMusic(overrideClip);
    }

    private void FadeOutAllAmbience(float duration = 0.5f)
    {
        foreach (var source in ambienceSources)
        {
            if (source.isPlaying) source.DOFade(0, duration);
        }
    }
    private void FadeInAllAmbience(float duration = 1.0f)
    {
        foreach (var source in ambienceSources)
        {
            if (source.clip != null) source.DOFade(1, duration); // Assumes volume is controlled by the source itself
        }
    }

    private AudioSource GetAvailableAmbienceSource()
    {
        // Find the first source that isn't currently playing a clip
        foreach (var source in ambienceSources)
        {
            if (source.clip == null) return source;
        }
        Debug.LogWarning("No available ambience sources!");
        return null;
    }
    // Call this when combat ends or you leave a special zone
    public void ReturnToSceneMusic()
    {
        if (musicSource.clip == _currentSceneMusic) return; // Don't restart if already playing
        FadeBetweenMusic(_currentSceneMusic);
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        musicSource.DOFade(0f, fadeDuration);
    }
}