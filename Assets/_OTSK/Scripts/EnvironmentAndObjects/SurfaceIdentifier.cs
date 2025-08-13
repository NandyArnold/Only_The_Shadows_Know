using UnityEngine;

public enum SurfaceType { Default, Wood, Stone, Dirt, Metal, Flesh }

public class SurfaceIdentifier : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Default;
}
