using UnityEngine;
//component to place in you scene to mark where the player can spawn.
public interface ISpawnPoint
{
    string SpawnPointTag { get; }
    Transform SpawnTransform { get; }
}