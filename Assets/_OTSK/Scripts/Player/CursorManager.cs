using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Tooltip("The crosshair PREFAB to be instantiated.")]
    [SerializeField] private GameObject gameplayCrosshairPrefab;

    private GameObject _crosshairInstance;
    private CursorState _currentState;

    public event Action<CursorState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       
    }

    private void Start()
    {
        // Subscribe to events in Start() to ensure other managers are ready.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        PersistentObject.OnCanvasReady += InitializeCrosshair;
    }

    private void OnDestroy()
    {
        // Always good practice to unsubscribe on destroy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
        PersistentObject.OnCanvasReady -= InitializeCrosshair;
    }

    // This is called once by the PersistentObject when the canvas is ready.
    private void InitializeCrosshair()
    {
        if (gameplayCrosshairPrefab != null && PersistentObject.Instance != null && _crosshairInstance == null)
        {
            _crosshairInstance = Instantiate(gameplayCrosshairPrefab, PersistentObject.Instance.transform);
        }
    }

    // This is called by GameManager when the scene changes state.
    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Gameplay:
                SetState(CursorState.Gameplay);
                break;
            case GameState.Menu:
            case GameState.Cutscene:
            case GameState.Loading:
                SetState(CursorState.UI);
                break;
        }
    }

    // This is the single source of truth for setting the cursor's appearance.
    public void SetState(CursorState newState)
    {
        _currentState = newState;
        switch (_currentState)
        {
            case CursorState.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (_crosshairInstance != null) _crosshairInstance.SetActive(true);
                break;

            case CursorState.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (_crosshairInstance != null) _crosshairInstance.SetActive(false);
                break;

            case CursorState.Targeting:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false; // Cursor is hidden, player aims with decal
                if (_crosshairInstance != null) _crosshairInstance.SetActive(false);
                break;
        }
        OnStateChanged?.Invoke(_currentState);
    }

    public void ToggleUIMode()
    {
        SetState(_currentState == CursorState.Gameplay ? CursorState.UI : CursorState.Gameplay);
    }
}