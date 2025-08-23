// Create this new script, UIPanelRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public class UIPanelRegistry : MonoBehaviour, IResettable
{
    public static UIPanelRegistry Instance { get; private set; }

    private readonly Dictionary<PanelID, GameObject> _panelRegistry = new Dictionary<PanelID, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPanel(PanelID id, GameObject panelObject)
    {
        if (!_panelRegistry.ContainsKey(id))
        {
            _panelRegistry.Add(id, panelObject);
        }
    }

    public GameObject GetPanel(PanelID id)
    {
        _panelRegistry.TryGetValue(id, out GameObject panel);
        return panel;
    }
    public void ResetState()
    {
        _panelRegistry.Clear();
        Debug.Log("<color=orange>UIPanelRegistry state has been reset.</color>");
    }
}