// Create this new script
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DeathZoneController : MonoBehaviour
{
    private LineRenderer _line;
    private Transform _playerHand;
    private Transform _targetSocket;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void Initialize(Transform playerHand, Transform targetSocket)
    {
        _playerHand = playerHand;
        _targetSocket = targetSocket;
        _line.useWorldSpace = true;
    }

    private void Update()
    {
        // Continuously draw a line from the player's hand to the target socket
        if (_playerHand != null && _targetSocket != null)
        {
            _line.SetPosition(0, _playerHand.position);
            _line.SetPosition(1, _targetSocket.position);
        }
    }
}