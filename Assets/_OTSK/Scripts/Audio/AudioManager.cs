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

    // --- Music Control ---

    private void FadeBetweenMusic(AudioClip newClip, float volume, float duration = 1.0f)
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
        newSource.Play();

        activeSource.DOFade(0f, duration);
        yield return newSource.DOFade(targetVolume, duration).WaitForCompletion();

        activeSource.Stop();
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

        if (_isGameOver) 
        { 
            targetClip = gameOverMusic; targetVolume = gameOverMusicVolume;
        }
        else if (_currentThreatState == ThreatState.Combat)
        { 
            targetClip = combatMusic; targetVolume = combatMusicVolume; 
        }
        else if (_currentThreatState == ThreatState.Alert)
        {
            targetClip = alertMusic; targetVolume = alertMusicVolume; 
        }
        else if (_overrideMusic != null) 
        {
            targetClip = _overrideMusic; targetVolume = _overrideMusicVolume;
        }
        else 
        {
            targetClip = _currentSceneMusic; targetVolume = _currentSceneMusicVolume; 
        }

        if (targetClip != null && _activeMusicSource.clip != targetClip)
        {
            FadeBetweenMusic(targetClip, targetVolume);
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

    //private void PlayMusicTrack(AudioClip clip)
    //{
    //    if (musicSource.clip == clip) return;

    //    if (_activeMusicFade != null) StopCoroutine(_activeMusicFade);
    //    _activeMusicFade = StartCoroutine(FadeBetweenMusicRoutine(clip, 1.0f));
    //}

   

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


    //public void PlayGameOverMusic()
    //{
    //    if (_activeMusicFade != null) StopCoroutine(_activeMusicFade);
    //    FadeOutAllAmbience(0.5f); // Fade out ambient sounds
    //    musicSource.DOKill(); // Instantly stop any current fades
    //    PlayMusicTrack(gameOverMusic);
    //}

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
}