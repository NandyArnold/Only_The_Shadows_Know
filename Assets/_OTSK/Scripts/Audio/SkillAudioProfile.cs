// SkillAudioProfile.cs
using UnityEngine;

[System.Serializable]
public class SkillAudioProfile
{
    [Header("Sound Sequence")]
    [Tooltip("A one-shot sound played at the start of the cast/channel.")]
    public SoundDefinition castStartSound;

    [Header("Channel Loop Settings")]
    [Tooltip("A looping sound played during the channel.")]
    public AudioClip channelLoopSound;
    [Tooltip("Delay in seconds after the cast starts before the loop begins.")]
    public float loopStartDelay = 0f;
    [Range(0f, 1f)] public float channelVolume = 0.8f;
    [Range(0.5f, 1.5f)] public float channelPitch = 1.0f;
    [Tooltip("How long it takes for the loop to fade in. Set to 0 for an instant start.")]
    public float loopFadeInDuration = 0.5f;
    [Tooltip("How long it takes for the loop to fade out. Set to 0 for an instant stop.")]
    public float loopFadeOutDuration = 0.5f;


    [Header("End Sound")]
    [Tooltip("A one-shot sound played when the cast/channel stops.")]
    public SoundDefinition castEndSound;
}