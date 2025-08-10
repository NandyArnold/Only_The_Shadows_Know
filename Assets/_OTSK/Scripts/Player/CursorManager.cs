// CursorManager.cs - FINAL SIMPLIFIED VERSION
using System;
using UnityEngine;
using System.Collections;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private CursorState _currentState;
 
    public event Action<CursorState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
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
                SetState(CursorState.UI);
                break;
            case GameState.Cutscene:
            case GameState.Loading:
                SetState(CursorState.Loading);
                break;
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
            case CursorState.Loading:
                Cursor.lockState = CursorLockMode.None;
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