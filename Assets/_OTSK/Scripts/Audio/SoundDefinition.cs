// SoundDefinition.cs
using UnityEngine;

[System.Serializable]
public class SoundDefinition
{
    public AudioClip[] clips;

    [Range(0f, 1f)] public float minVolume = 0.9f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;
    [Space]
    [Range(0.5f, 1.5f)] public float minPitch = 0.95f;
    [Range(0.5f, 1.5f)] public float maxPitch = 1.05f;

    // A helper method to play the sound easily
    public void Play(Transform emitter)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        float randomVolume = Random.Range(minVolume, maxVolume);
        float randomPitch = Random.Range(minPitch, maxPitch);

        SoundEffectManager.Instance.PlaySoundAtPoint(randomClip, emitter.position, randomVolume, randomPitch);
    }
}