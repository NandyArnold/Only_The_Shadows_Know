// LoadingScreenController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private List<string> loadingTips;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Show a random tip
        if (loadingTips != null && loadingTips.Count > 0)
        {
            tipText.text = "Tip: " + loadingTips[Random.Range(0, loadingTips.Count)];
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
    }
}