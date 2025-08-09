// CursorManager.cs - FINAL SIMPLIFIED VERSION
using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private CursorState _currentState;
    public event Action<CursorState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

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
            case GameState.GameOver:
                SetState(CursorState.UI);
                break;
        }
    }

    public void SetState(CursorState newState)
    {
        _currentState = newState;
        switch (_currentState)
        {
            case CursorState.Gameplay:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case CursorState.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case CursorState.Targeting:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                break;
        }
        OnStateChanged?.Invoke(_currentState);
    }

    public void ToggleUIMode()
    {
        SetState(_currentState == CursorState.Gameplay ? CursorState.UI : CursorState.Gameplay);
    }
}