// EndwalkerVisuals.cs
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class EndwalkerVisuals : MonoBehaviour
{
    public static EndwalkerVisuals Instance { get; private set; }
    [Header("Volume References")]
    [Tooltip("The Volume component for your game's normal look.")]
    [SerializeField] private Volume globalVolume;
    [Tooltip("The Volume component for the desaturated Endwalker effect.")]
    [SerializeField] private Volume endwalkerVolume;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (globalVolume != null) globalVolume.weight = 1f;
        // Ensure the effect is off when the game starts
        if (endwalkerVolume != null) endwalkerVolume.weight = 0f;
    }


    public void SetEndwalkerActive(bool isActive)
    {
        if (globalVolume == null || endwalkerVolume == null) return;

        // Animate the weights to crossfade between the two volumes.
        if (isActive)
        {
            DOTween.To(() => globalVolume.weight, x => globalVolume.weight = x, 0f, fadeDuration);
            DOTween.To(() => endwalkerVolume.weight, x => endwalkerVolume.weight = x, 1f, fadeDuration);
        }
        else
        {
            DOTween.To(() => globalVolume.weight, x => globalVolume.weight = x, 1f, fadeDuration);
            DOTween.To(() => endwalkerVolume.weight, x => endwalkerVolume.weight = x, 0f, fadeDuration);
        }
    }
}