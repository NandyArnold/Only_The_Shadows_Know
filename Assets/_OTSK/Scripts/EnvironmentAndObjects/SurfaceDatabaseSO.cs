// Create a new script: SurfaceDatabaseSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Surface Database", menuName = "Only The Shadows Know/Audio/Surface Database")]
public class SurfaceDatabaseSO : ScriptableObject
{
    public List<SurfaceAudioSO> surfaceAudioData;

    private Dictionary<SurfaceType, SurfaceAudioSO> _database;

    public SurfaceAudioSO GetAudioData(SurfaceType surfaceType)
    {
        // Build the database on first access for efficiency
        if (_database == null)
        {
            _database = new Dictionary<SurfaceType, SurfaceAudioSO>();
            foreach (var data in surfaceAudioData)
            {
                _database[data.surface] = data;
            }
        }

        // Try to find the specific surface, fall back to Default if not found
        if (_database.TryGetValue(surfaceType, out var audioData))
        {
            return audioData;
        }
        return _database[SurfaceType.Default];
    }
}