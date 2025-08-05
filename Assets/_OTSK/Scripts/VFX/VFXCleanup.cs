using UnityEngine;
public class VFXCleanup : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.15f;
    void Start() => Destroy(gameObject, lifeTime);
}