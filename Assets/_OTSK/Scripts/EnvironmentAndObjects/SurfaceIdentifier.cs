using UnityEngine;

public enum SurfaceType { Default, Wood, Stone, Dirt, Metal, Flesh, Water }

public class SurfaceIdentifier : MonoBehaviour
{
    public SurfaceType surfaceType = SurfaceType.Default;
}
