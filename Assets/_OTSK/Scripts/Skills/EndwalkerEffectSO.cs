// In EndwalkerEffectSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "FX_Endwalker", menuName = "Only The Shadows Know/Skills/Effects/Endwalker Effect")]
public class EndwalkerEffectSO : SkillEffectSO
{
    //[Header("Endwalker Settings")]
    //[Tooltip("The looping ambient sound for the spirit world.")]
    //public AudioClip endwalkerAmbience;
    //public float ambienceVolume = 0.7f;
    //public float ambienceFadeDuration = 1.5f;
    public override void Activate(GameObject caster)
    {
        if (caster.TryGetComponent<PlayerController>(out var playerController))
        {
            // First, check if we are currently IN the Endwalker state.
            if (playerController.IsInEndwalkerState)
            {
                // If we are, it means we need to turn it OFF.
                // Tell the AudioManager to stop the looping sound.
                AudioManager.Instance.StopChanneledSound(audioProfile.loopFadeOutDuration);
            }
            else
            {
                // If we are NOT, it means we need to turn it ON.
                // Tell the AudioManager to start the looping sound.
                if (audioProfile != null && audioProfile.channelLoopSound != null)
                {
                    AudioManager.Instance.PlayChanneledSound(
                        audioProfile.channelLoopSound,
                        audioProfile.channelVolume,
                        audioProfile.channelPitch,
                        audioProfile.loopFadeInDuration
                    );
                }
            }

            // Finally, after handling the audio, toggle the actual state.
            playerController.SetEndwalkerState(!playerController.IsInEndwalkerState);
        }
    }
}