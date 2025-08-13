using UnityEngine;

// A simple container for our sound data
[System.Serializable]
public class SurfaceSound
{
    public SurfaceType surface;
    public AudioClip audioClip;
    public float noiseIntensity = 20f;
}

[CreateAssetMenu(fileName = "SurfaceAudioData", menuName = "Only The Shadows Know/Audio/Surface Audio Data")]
public class SurfaceAudioDataSO : ScriptableObject
{
    public SurfaceSound[] surfaceSounds;

    public SurfaceSound GetSound(SurfaceType surfaceType)
    {
        foreach (var sound in surfaceSounds)
        {
            if (sound.surface == surfaceType)
            {
                return sound;
            }
        }
        // Return the default sound if no specific sound is found
        return surfaceSounds[0];
    }
}