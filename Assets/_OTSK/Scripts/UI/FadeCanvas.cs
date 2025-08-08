// FadeCanvas.cs
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvas : MonoBehaviour
{
    public static FadeCanvas Instance { get; private set; }

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration)
    {
        gameObject.SetActive(true);
        _canvasGroup.DOFade(1f, duration).SetUpdate(true);
    }

    public void FadeOut(float duration)
    {
        _canvasGroup.DOFade(0f, duration).SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }
}