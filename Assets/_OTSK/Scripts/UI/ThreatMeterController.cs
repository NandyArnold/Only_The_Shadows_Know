// ThreatMeterController.cs
using UnityEngine;
using DG.Tweening;

public class ThreatMeterController : MonoBehaviour
{
    private enum ThreatState { Safe, Alert, Combat }

    [Header("UI References")]
    [SerializeField] private GameObject safeIcon;
    [SerializeField] private GameObject alertIcon;
    [SerializeField] private GameObject combatIcon;

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
        SetState(ThreatState.Safe, true);
    }

    private void Update()
    {
        // Determine the correct state
        ThreatState targetState;
        if (CombatManager.Instance.IsPlayerInCombat)
        {
            targetState = ThreatState.Combat;
        }
        else if (EnemyManager.Instance.IsAnyEnemyAlerted())
        {
            targetState = ThreatState.Alert;
        }
        else
        {
            targetState = ThreatState.Safe;
        }

        // If the state needs to change, update the UI
        if (targetState != _currentState)
        {
            SetState(targetState);
        }
    }

    private void SetState(ThreatState newState, bool instant = false)
    {
        if (_currentState == newState && !instant) return;

        _currentState = newState;

        // Stop any previous looping animations
        if (_activeTween != null) _activeTween.Kill();

        float duration = instant ? 0f : fadeDuration;

        // --- HIDE INACTIVE ICONS ---
        if (_currentState != ThreatState.Safe) _safeCG.DOFade(0f, duration);
        if (_currentState != ThreatState.Alert) _alertCG.DOFade(0f, duration);
        if (_currentState != ThreatState.Combat) _combatCG.DOFade(0f, duration);

        // --- SHOW AND ANIMATE THE ACTIVE ICON ---
        switch (newState)
        {
            case ThreatState.Safe:
                _safeCG.DOFade(1f, duration);
                break;
            case ThreatState.Alert:
                _alertCG.DOFade(1f, duration);
                _activeTween = alertIcon.transform.DOShakeRotation(1f, new Vector3(0, 0, alertShakeStrength), 10, 90, false).SetLoops(-1);
                break;
            case ThreatState.Combat:
                _combatCG.DOFade(1f, duration);
                _activeTween = combatIcon.transform.DOScale(combatPulseScale, combatPulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }
}