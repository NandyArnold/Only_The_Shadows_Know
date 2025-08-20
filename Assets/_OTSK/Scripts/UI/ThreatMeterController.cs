// ThreatMeterController.cs
using UnityEngine;
using DG.Tweening;

public class ThreatMeterController : MonoBehaviour
{
  

    [Header("UI References")]
    [SerializeField] private GameObject safeIcon;
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private GameObject combatIcon;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip alertMusic;
    [SerializeField] private AudioClip combatMusic;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float alertShakeStrength = 5f;
    [SerializeField] private float combatPulseScale = 1.1f;
    [SerializeField] private float combatPulseDuration = 1f;

    private CanvasGroup _safeCG, _alertCG, _combatCG;
    private ThreatState _currentState = ThreatState.Safe;
    private Tween _activeTween;

    private void Awake()
    {
        _safeCG = safeIcon.GetComponent<CanvasGroup>();
        _alertCG = alertIcon.GetComponent<CanvasGroup>();
        _combatCG = combatIcon.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Set initial state
        SetVisualState(ThreatState.Safe, true);
    }

    private void Update()
    {
        // Determine the correct state
        ThreatState targetState;
        if (CombatManager.Instance.IsPlayerInCombat) targetState = ThreatState.Combat;
        else if (EnemyManager.Instance.IsAnyEnemyAlerted()) targetState = ThreatState.Alert;
        else targetState = ThreatState.Safe;

        if (targetState != _currentState)
        {
            _currentState = targetState;

            // This is the only line that should handle audio
            AudioManager.Instance.SetThreatState(_currentState);

            // This now only handles visuals
            SetVisualState(_currentState);
        }
    }

    private void SetVisualState(ThreatState newState, bool instant = false)
    {
        // Stop any previous looping animations
        if (_activeTween != null) _activeTween.Kill();

        float duration = instant ? 0f : fadeDuration;

        // --- HIDE INACTIVE ICONS ---
        _safeCG.DOFade((newState == ThreatState.Safe) ? 1f : 0f, duration);
        _alertCG.DOFade((newState == ThreatState.Alert) ? 1f : 0f, duration);
        _combatCG.DOFade((newState == ThreatState.Combat) ? 1f : 0f, duration);

        // --- SHOW AND ANIMATE THE ACTIVE ICON ---
        switch (newState)
        {
            case ThreatState.Alert:
                _activeTween = alertIcon.transform.DOShakeRotation(1f, new Vector3(0, 0, alertShakeStrength), 10, 90, false).SetLoops(-1);
                break;
            case ThreatState.Combat:
                _activeTween = combatIcon.transform.DOScale(combatPulseScale, combatPulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }
}