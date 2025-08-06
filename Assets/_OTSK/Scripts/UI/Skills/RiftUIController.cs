using UnityEngine;
using TMPro;

public class RiftUIController : MonoBehaviour
{
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Color inRangeColor = Color.cyan;
    [SerializeField] private Color outOfRangeColor = Color.red;

    private Transform _playerTransform;
    private Vector3? _riftPosition = null;
    private float _maxRange;

    private void OnEnable()
    {
        EventManager.Instance.OnRiftPlaced += HandleRiftPlaced;
        EventManager.Instance.OnRiftCancelledOrUsed += HandleRiftRemoved;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnRiftPlaced -= HandleRiftPlaced;
        EventManager.Instance.OnRiftCancelledOrUsed -= HandleRiftRemoved;
    }

    private void Start()
    {
        // Subscribe to the GameManager to know when the player is ready.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
        }

        statusPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // It's good practice to also unsubscribe here in case this object is destroyed.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;
        }
    }

    // This method is called by the GameManager once the player has been spawned.
    private void HandlePlayerRegistered(PlayerController player)
    {
        _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_riftPosition.HasValue && _playerTransform != null)
        {
            float distance = Vector3.Distance(_playerTransform.position, _riftPosition.Value);
            if (distance <= _maxRange)
            {
                statusText.text = "Rift: In Range";
                statusText.color = inRangeColor;
            }
            else
            {
                statusText.text = "Rift: Out of Range";
                statusText.color = outOfRangeColor;
            }
        }
    }

    private void HandleRiftPlaced(Vector3 position, float maxRange)
    {
        _riftPosition = position;
        _maxRange = maxRange;
        statusPanel.SetActive(true);
    }
    private void HandleRiftRemoved()
    {
        _riftPosition = null;
        statusPanel.SetActive(false);
    }
}