using UnityEngine;
public class VFXCleanup : MonoBehaviour
{
    public void BeginCleanup(float duration)
    {
        Destroy(gameObject, duration);
    }
}
