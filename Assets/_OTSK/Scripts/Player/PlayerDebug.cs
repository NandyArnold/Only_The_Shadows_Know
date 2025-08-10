using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;
    private Invulnerability _invulnerability;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _invulnerability = GetComponent<Invulnerability>();

        if (_inputHandler == null)
            Debug.LogError("PlayerDebug: Could not find PlayerInputHandler component!");
        if (_invulnerability == null)
            Debug.LogError("PlayerDebug: Could not find Invulnerability component!");
    }

    private void OnEnable()
    {
        if (_inputHandler != null)
        {
            Debug.Log("[PlayerDebug] Subscribing to OnToggleInvulnerabilityInput event.");
            _inputHandler.OnToggleInvulnerabilityInput += ToggleInvulnerability;
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnToggleInvulnerabilityInput -= ToggleInvulnerability;
        }
    }

    private void ToggleInvulnerability()
    {
        if (_invulnerability == null) return;

        _invulnerability.IsInvulnerable = !_invulnerability.IsInvulnerable;

        Debug.Log($"<color=cyan>[PlayerDebug] ToggleInvulnerability called! Player Invulnerability is now: {_invulnerability.IsInvulnerable}</color>");
    }
}