using UnityEngine;

public class RevealableEntity : MonoBehaviour
{
    public RevealableType Type { get; private set; }

    // This is a private field for setting a default/override in the Inspector for non-enemy objects.
    [SerializeField] private RevealableType type = RevealableType.None;

    private void Awake()
    {
        // Set the default type from the Inspector.
        Type = type;
    }

    // This method, called by Enemy.cs, will OVERRIDE the Inspector setting.
    public void Initialize(EnemyConfigSO config)
    {
        Type = config.revealableType;
    }
}