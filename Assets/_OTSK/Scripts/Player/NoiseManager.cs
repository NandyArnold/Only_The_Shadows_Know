using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
    }

    // A universal method to generate noise from any source
    public void GenerateNoise(Vector3 position, float intensity, GameObject sourceObject = null)
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.BroadcastSound(position, intensity, sourceObject);
        }
    }
}