// SurfaceAudioSO.cs
using UnityEngine;

//  A class to hold all settings for one sound type ---
[System.Serializable]
public class MovementSound
{
    public AudioClip[] clips;

    [Range(0f, 1f)] public float minVolume = 0.8f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;
    [Space]
    [Range(0.5f, 1.5f)] public float minPitch = 0.9f;
    [Range(0.5f, 1.5f)] public float maxPitch = 1.1f;
}

[CreateAssetMenu(fileName = "New Surface Audio", menuName = "Only The Shadows Know/Audio/Surface Audio SO")]
public class SurfaceAudioSO : ScriptableObject
{
    public SurfaceType surface;

    // --- CHANGE: We now use our new MovementSound class ---
    [Header("Sound Definitions")]
    public MovementSound walkSound;
    public MovementSound runSound;
    public MovementSound crouchSound;
}