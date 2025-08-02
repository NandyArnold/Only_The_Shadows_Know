// CombatManager.cs
using System;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public bool IsPlayerInCombat { get; private set; } = false;

    public event Action OnCombatStart;
    public event Action OnCombatEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void EnterCombat()
    {
        if (IsPlayerInCombat) return;

        IsPlayerInCombat = true;
        OnCombatStart?.Invoke();
        Debug.Log("<color=red>COMBAT STARTED</color>");
    }

    public void ExitCombat()
    {
        if (!IsPlayerInCombat) return;

        IsPlayerInCombat = false;
        OnCombatEnd?.Invoke();
        Debug.Log("<color=green>COMBAT ENDED</color>");
    }
}