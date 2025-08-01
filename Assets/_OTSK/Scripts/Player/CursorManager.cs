// In CursorManager.cs - REVISED
using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private GameObject crosshairPrefab;
    private GameObject _crosshairInstance;

    // ADD THIS: A field to remember the current scene type.
    private SceneType _currentSceneType;

    public event Action<bool> OnCursorLockStateChanged;

    public bool isCursorLocked = true; // Default to locked cursor state.
    // ... (Awake and InstantiateCrosshair methods are unchanged) ...
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InstantiateCrosshair();
    }

    private void InstantiateCrosshair()
    {
        PersistentObject persistentCanvasObject = UnityEngine.Object.FindFirstObjectByType<PersistentObject>();
        if (crosshairPrefab != null && persistentCanvasObject != null)
        {
            _crosshairInstance = Instantiate(crosshairPrefab, persistentCanvasObject.transform);
            _crosshairInstance.SetActive(false);
        }
    }


    private void OnEnable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded += HandleSceneLoaded;
        }
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.OnSceneLoaded -= HandleSceneLoaded;
        }
    }

    private void HandleSceneLoaded(SceneDataSO sceneData)
    {
        // ADD THIS LINE: Store the current scene type.
        _currentSceneType = sceneData.sceneType;

        switch (sceneData.sceneType)
        {
            case SceneType.Gameplay:
                LockCursor();
                break;
            case SceneType.Menu:
                UnlockCursor();
                break;
        }
    }

    // ADD THIS METHOD BACK: The new, smarter toggle method.
    public void ToggleCursorMode()
    {
        isCursorLocked = !isCursorLocked;
        ApplyCursorState();
        //// Guard Clause: Only allow manual toggling in a gameplay scene.
        //if (_currentSceneType != SceneType.Gameplay) return;

        //// If it's currently locked, unlock it. Otherwise, lock it.
        //if (Cursor.lockState == CursorLockMode.Locked)
        //{
        //    UnlockCursor();
        //}
        //else
        //{
        //    LockCursor();
        //}
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (_crosshairInstance != null) _crosshairInstance.SetActive(true);
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_crosshairInstance != null) _crosshairInstance.SetActive(false);
    }

    private void ApplyCursorState()
    {
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (_crosshairInstance != null) _crosshairInstance.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (_crosshairInstance != null) _crosshairInstance.SetActive(false);
        }

        // Fire the event to notify other systems (like the Input Handler).
        OnCursorLockStateChanged?.Invoke(isCursorLocked);
    }
}