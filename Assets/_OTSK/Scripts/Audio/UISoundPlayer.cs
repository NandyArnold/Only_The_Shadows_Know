// Create this new script: UISoundPlayer.cs
using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
    public static UISoundPlayer Instance { get; private set; }

    [SerializeField] private UISoundsSO uiSounds;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Create a simple public method for each sound
    public void PlayHoverSound() => Play(uiSounds.hover);
    public void PlayClickConfirmSound() => Play(uiSounds.clickConfirm);
    public void PlayClickBackSound() => Play(uiSounds.clickBack);
    public void PlayErrorSound() => Play(uiSounds.error);
    public void PlayMenuOpenSound() => Play(uiSounds.menuOpen);
    public void PlayMenuCloseSound() => Play(uiSounds.menuClose);
    public void PlayNewObjectiveSound() => Play(uiSounds.newObjective);

    public void PlayToggleSound() => Play(uiSounds.toggle);

    private void Play(AudioClip clip)
    {
        if (clip != null)
        {
            // Use the dedicated uiSfxSource from our AudioManager
            AudioManager.Instance.PlayUISound(clip);
        }
    }
}