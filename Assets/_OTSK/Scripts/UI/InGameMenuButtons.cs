using UnityEngine;

public class InGameMenuButtons : MonoBehaviour
{
    public void OnResumeClicked()
    {
        // Tell the GameManager to go back to the Gameplay state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }
    }

    public void OnSaveGameClicked()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame("manual_save_1"); 
        }
    }

    public void OnLoadGameClicked()
    {
        // Logic to open a "Load Game" screen
        Debug.Log("Load Game clicked.");

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame("manual_save_1"); 
        }
    }
    public void OnOptionsClicked()
    {
        // Logic to open an "Options" screen
        Debug.Log("Options clicked.");
        // Here you would typically open a UI panel for options
    }

    public void OnMainMenuClicked()
    {
        // Logic to return to the main menu
        Debug.Log("Main Menu clicked.");
        
        //  loads the main menu scene 
        
    }

    public void OnExitGameClicked()
    {
        // Logic to exit the game
        Debug.Log("Exit Game clicked.");
        Application.Quit();
    }




    // ... (Add methods for Load, Options, and Main Menu)
}
