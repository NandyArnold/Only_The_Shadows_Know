// OptionsMenu.cs
using UnityEngine;

public class InGame_Menu : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to the GameManager to know when the state changes.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
        // Ensure it's hidden at start, just in case.
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        // Show this panel only when the game state is Menu.
        gameObject.SetActive(newState == GameState.Menu);
    }
}