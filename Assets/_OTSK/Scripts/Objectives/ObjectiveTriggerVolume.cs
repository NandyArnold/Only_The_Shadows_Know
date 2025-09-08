// ObjectiveTriggerVolume.cs (CORRECTED)

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveTriggerVolume : MonoBehaviour, ISaveable 
{

    [System.Serializable] 
    public struct TriggerVolumeSaveData
    {
        public bool wasTriggered;
    }

    [Header("Configuration")]
    [Tooltip("A unique ID for this trigger volume.")]
    [SerializeField] private string triggerID;

    [Header("Event Channel")]
    [Tooltip("The event to raise when the player enters this volume.")]
    [SerializeField] private StringGameEvent onPlayerEnteredEvent;

    private UniqueID _uniqueID;

    private void Awake()
    {
        _uniqueID = GetComponent<UniqueID>();
    }


    // --- ISaveable Implementation ---
    public string UniqueID => _uniqueID.ID;

    public object CaptureState()
    {
        return new TriggerVolumeSaveData
        {
            wasTriggered = !gameObject.activeSelf
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (TriggerVolumeSaveData)state;
        if (saveData.wasTriggered)
        {
            gameObject.SetActive(false);
        }
    }
    // -----------------------------

    private void Start() // Use Start for registration
    {
        SaveableEntityRegistry.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (SaveableEntityRegistry.Instance != null)
        {
            SaveableEntityRegistry.Instance.Unregister(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered objective trigger volume: {triggerID}");

            if (onPlayerEnteredEvent != null)
            {
                // Raise the ScriptableObject event, passing our ID
                onPlayerEnteredEvent.Raise(triggerID);
            }

            // Deactivate to prevent it from firing multiple times.
            gameObject.SetActive(false);
        }
    }
}