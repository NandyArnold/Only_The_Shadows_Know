using UnityEngine;
using System.Collections; // Make sure you have DOTween installed
using DG.Tweening; // Make sure you have DOTween installed
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private float _defaultFixedDeltaTime;
    private Coroutine _timeScaleCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void DoTimeScale(float targetScale, float duration)
    {
        if (_timeScaleCoroutine != null)
        {
            StopCoroutine(_timeScaleCoroutine);
        }
        _timeScaleCoroutine = StartCoroutine(TimeScaleRoutine(targetScale, duration));
        DG.Tweening.DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 2, 0.4f).SetEase(DG.Tweening.Ease.InQuad).SetUpdate(true);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        // Adjust fixedDeltaTime to keep physics consistent with the new time scale
        Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
    }
    private IEnumerator TimeScaleRoutine(float targetScale, float duration)
    {
        float startScale = Time.timeScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Use unscaledDeltaTime to ensure the coroutine runs in real-time,
            // ignoring the game's slow motion.
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;

            float newScale = Mathf.SmoothStep(startScale, targetScale, t);
            Time.timeScale = newScale;
            Time.fixedDeltaTime = _defaultFixedDeltaTime * newScale;

            yield return null;
        }

        // Ensure the final value is set correctly.
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * targetScale;
    }
}
