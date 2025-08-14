using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{

   

    public void OnLoadGameClicked()
    {
        // We will add logic here to open a "Load Game" screen later
        Debug.Log("Load Game clicked.");
    }

    public void OnOptionsClicked()
    {
        // We will add logic here to open an "Options" screen later
        Debug.Log("Options clicked.");
    }

    public void OnExitGameClicked()
    {
        Application.Quit();
    }
}