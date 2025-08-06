using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
    }

    // Input Events
    public event Action OnCancelActionInput;
    public void CancelActionInput() => OnCancelActionInput?.Invoke();

    // Skill Events
    public event Action<Vector3, float> OnRiftPlaced;
    public void RiftPlaced(Vector3 position, float maxRange)
    {
        Debug.Log("EventManager: RiftPlaced method called.");
        if (OnRiftPlaced != null)
        {
            Debug.Log($"EventManager: Invoking OnRiftPlaced. Number of listeners: {OnRiftPlaced.GetInvocationList().Length}");
            OnRiftPlaced.Invoke(position, maxRange);
        }
        else
        {
            Debug.LogWarning("EventManager: OnRiftPlaced event was fired, but it has no listeners.");
        }
    }

    public event Action OnRiftCancelledOrUsed;
    public void RiftCancelledOrUsed() => OnRiftCancelledOrUsed?.Invoke();
}