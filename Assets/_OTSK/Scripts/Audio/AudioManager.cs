using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // We'll use DOTween for smooth fading



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


    [Header("Priority Music")]
    [SerializeField] private AudioClip alertMusic;
    [SerializeField][Range(0f, 1f)] private float alertMusicVolume = 0.9f;
    [SerializeField] private AudioClip combatMusic;
    [SerializeField][Range(0f, 1f)] private float combatMusicVolume = 1f;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField][Range(0f, 1f)] private float gameOverMusicVolume = 1f;

   


    private AudioSource _activeMusicSource;
    private ThreatState _currentThreatState = ThreatState.Safe;

    private AudioClip _currentSceneMusic; // The default track for the loaded level
    private float _currentSceneMusicVolume = 1f;

    private AudioClip _overrideMusic;
    private float _overrideMusicVolume = 1f;

    private Coroutine _activeMusicFade;
    private bool _isGameOver = false;


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

    public void PlayChanneledSound(AudioClip clip)
    {
        if (channelSfxSource == null || clip == null) return;

        channelSfxSource.clip = clip;
        channelSfxSource.loop = true;
        channelSfxSource.pitch = 1f; // Ensure pitch is normal at the start
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
    public void StopChanneledSound()
    {
        if (channelSfxSource != null)
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

    public void SetOverrideMusic(AudioClip overrideClip, float volume)
    {
        _overrideMusic = overrideClip;
        _overrideMusicVolume = volume;
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
        if (newState == ThreatState.Safe) FadeInAllAmbience();
        else FadeOutAllAmbience();
    }
    private void UpdateMusic()
    {
        AudioClip targetClip = null;
        float targetVolume = 1f;

        if (_isGameOver) { targetClip = gameOverMusic; targetVolume = gameOverMusicVolume; }
        else if (_currentThreatState == ThreatState.Combat) { targetClip = combatMusic; targetVolume = combatMusicVolume; }
        else if (_currentThreatState == ThreatState.Alert) { targetClip = alertMusic; targetVolume = alertMusicVolume; }
        else if (_overrideMusic != null) { targetClip = _overrideMusic; targetVolume = _overrideMusicVolume; }
        else { targetClip = _currentSceneMusic; targetVolume = _currentSceneMusicVolume; }

        if (targetClip != null)
        {
            Debug.Log($"<color=yellow>[AudioManager]</color> UpdateMusic decided the target is '{targetClip.name}' at volume '{targetVolume}'.");
        }
        if (targetClip != null && (_activeMusicSource.clip != targetClip || !_activeMusicSource.isPlaying))
        {
            FadeBetweenMusic(targetClip, targetVolume, 1.0f); // Pass all 3 arguments
        }
    }


    public void EnterAmbienceZone(AudioClip clip, float volume, float fadeDuration)
    {
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
                });
                return;
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
        // This method is tricky now. We don't know the target volume for each sound.
        // For now, let's leave this blank as the Enter/Exit logic handles fading.
        // A more complex system could store the target volume for each active sound.
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
}