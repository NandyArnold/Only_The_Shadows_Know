// VFXManager.cs
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private GameObject revealVFXPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void PlayRevealEffect(Vector3 position)
    {
        if (revealVFXPrefab != null)
        {
            Instantiate(revealVFXPrefab, position, Quaternion.identity);
        }
    }
}