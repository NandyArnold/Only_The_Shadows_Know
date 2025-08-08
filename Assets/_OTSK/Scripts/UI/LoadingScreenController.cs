// LoadingScreenController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }
    [Header("UI Elements")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI progressText;
  
    [Header("Settings")]
    [SerializeField] private List<string> loadingTips;
    [SerializeField] private float loadingTextFadeDuration = 1.5f;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Show a random tip
        if (loadingTips != null && loadingTips.Count > 0 && tipText != null)
        {
            tipText.text = "Tip: " + loadingTips[Random.Range(0, loadingTips.Count)];
        }

        // Create a looping fade animation for the "Loading..." text
        if (loadingText != null)
        {
            loadingText.DOFade(0.2f, loadingTextFadeDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBarImage != null)
        {
            // Update the fill amount of the radial image
            progressBarImage.fillAmount = progress;
        }
        if (progressText != null)
        {
            // Update the percentage text (formatted to show no decimals)
            progressText.text = $"{(progress * 100f):F0}%";
        }
    }
}