using UnityEngine;
using System.Collections.Generic;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }
    public bool Muted { get; set; } = true;

    [Header("Pooling Settings")]
    [SerializeField] private SoundEffect soundEffectPrefab; // The prefab we'll create
    [SerializeField] private int initialPoolSize = 20;

    private Queue<SoundEffect> _soundEffectPool;
    private bool _isEndwalkerActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        //DontDestroyOnLoad(gameObject);

        CreatePool();
    }

    private void CreatePool()
    {
        _soundEffectPool = new Queue<SoundEffect>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            SoundEffect newEffect = Instantiate(soundEffectPrefab, transform);
            newEffect.gameObject.SetActive(false);
            _soundEffectPool.Enqueue(newEffect);
        }
    }

    public void SetEndwalkerState(bool isActive)
    {
        _isEndwalkerActive = isActive;
    }

    // This is the new, primary method for playing sounds.
    public void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, SoundCategory category = SoundCategory.Normal)
    {
        if (Muted) return;
        if (_isEndwalkerActive && category == SoundCategory.Normal)
        {
            return; // Mute the sound
        }

        if (clip == null) return;

        // Get an object from the pool.
        if (_soundEffectPool.Count == 0)
        {
            Debug.LogWarning("Sound effect pool is empty. Consider increasing the pool size.");
            return;
        }

        SoundEffect effect = _soundEffectPool.Dequeue();

        // Position it, activate it, and play the sound.
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        effect.Play(clip, volume, pitch);
    }

    // This method is called by the SoundEffect component when it's done playing.
    public void ReturnToPool(SoundEffect effect)
    {
        effect.gameObject.SetActive(false);
        _soundEffectPool.Enqueue(effect);
    }

    // You can keep your special methods like this one.
    public void PlayInstaKillSound()
    {
        AudioClip instaKillClip = Resources.Load<AudioClip>("Audio/InstaKillSound");
        if (instaKillClip != null)
        {
            // Note: This now plays at the AudioManager's position (2D sound).
            // If you want it to be at the player's position, you'd pass in GameManager.Instance.Player.transform.position.
            PlaySoundAtPoint(instaKillClip, GameManager.Instance.Player.transform.position, 1f);
        }
        else
        {
            Debug.LogWarning("InstaKill sound clip not found!");
        }
    }
}