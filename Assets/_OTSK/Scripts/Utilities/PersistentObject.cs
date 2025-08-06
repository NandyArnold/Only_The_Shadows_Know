// In PersistentObject.cs
using System;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    public static PersistentObject Instance { get; private set; }
    
    // An event to announce that the persistent canvas is ready
    public static event Action OnCanvasReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Announce that this object is ready
        OnCanvasReady?.Invoke();
    }
}