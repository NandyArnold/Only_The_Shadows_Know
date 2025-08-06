using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private float _defaultFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        _defaultFixedDeltaTime = Time.fixedDeltaTime;
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
}