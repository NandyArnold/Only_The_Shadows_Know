using System.Collections.Generic;
using UnityEngine;

// This doesn't need to be in its own file, but it's cleaner if it is.
// It's the same enum from SurfaceIdentifier.cs
// public enum SurfaceType { Default, Wood, Stone, Dirt, Metal, Flesh }

[System.Serializable]
public class SurfaceImpactNoise
{
    public SurfaceType surface;
    public float noiseIntensity;
}

[CreateAssetMenu(fileName = "ImpactNoiseData_New", menuName = "Only The Shadows Know/AI/Impact Noise Data")]
public class ImpactNoiseDataSO : ScriptableObject
{
    [SerializeField] private SurfaceImpactNoise[] surfaceImpactNoises;

    // A default value for any surfaces we haven't defined
    [SerializeField] private float defaultNoiseIntensity = 20f;

    private Dictionary<SurfaceType, float> _noiseLookup;

    private void OnEnable()
    {
        _noiseLookup = new Dictionary<SurfaceType, float>();
        foreach (var noise in surfaceImpactNoises)
        {
            _noiseLookup[noise.surface] = noise.noiseIntensity;
        }
    }

    public float GetNoiseIntensity(SurfaceType surfaceType)
    {
        if (_noiseLookup.TryGetValue(surfaceType, out float intensity))
        {
            return intensity;
        }
        return defaultNoiseIntensity;
    }
}