// Create this new script, UIPanel.cs
using UnityEngine;

public enum PanelID { MainMenu, InGameMenu, GameOver, Tooltips, Options }

public class UIPanel : MonoBehaviour
{
    public PanelID panelID;
}