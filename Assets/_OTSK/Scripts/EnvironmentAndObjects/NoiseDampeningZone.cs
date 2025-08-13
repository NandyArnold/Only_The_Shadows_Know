using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoiseDampeningZone : MonoBehaviour
{
    public enum ZoneType
    {
        // For solid obstacles that block line-of-sound, like a wall.
        Occlusion,
        // For general areas where sound is muffled, like inside a building or thick fog.
        Area
    }

    [Tooltip("The type of dampening this zone provides.")]
    public ZoneType type = ZoneType.Area;

    [Tooltip("How much to reduce sound intensity. 0.5 = 50% quieter, 0 = completely silent.")]
    [Range(0f, 1f)]
    public float dampeningMultiplier = 0.5f;

   
}