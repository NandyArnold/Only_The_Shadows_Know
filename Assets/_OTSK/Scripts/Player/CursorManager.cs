using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Tooltip("The crosshair used for standard gameplay.")]
    [SerializeField] private GameObject gameplayCrosshairUI;

    private CursorState _currentState;
    public event System.Action<CursorState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // We now subscribe to events instead of using Start().
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    // This handler will be called automatically by the GameManager.
    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Gameplay:
                SetState(CursorState.Gameplay);
                break;
            case GameState.Menu:
            case GameState.Cutscene: // Also unlock cursor for cutscenes
            case GameState.Loading:  // And for loading screens
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
                if (gameplayCrosshairUI != null) gameplayCrosshairUI.SetActive(true);
                break;

            case CursorState.UI:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (gameplayCrosshairUI != null) gameplayCrosshairUI.SetActive(false);
                break;

            case CursorState.Targeting:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                if (gameplayCrosshairUI != null) gameplayCrosshairUI.SetActive(false);
                break;
        }
        OnStateChanged?.Invoke(_currentState);
    }

    public void ToggleUIMode()
    {
        SetState(_currentState == CursorState.Gameplay ? CursorState.UI : CursorState.Gameplay);
    }
}