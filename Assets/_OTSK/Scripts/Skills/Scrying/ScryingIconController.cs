// ScryingIconController.cs - Generic Version
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RevealableEntity))]
public class ScryingIconController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject scryingIconPrefab;
    [SerializeField] private TacticalIconAtlasSO iconAtlas;

    private GameObject scryingIconInstance;
    private RevealableEntity revealableEntity;
    private Enemy enemy; // Optional, for checking resistance



    private void OnEnable()
    {
        // Register this icon with the central system
        ScryingSystem.Instance?.RegisterIconController(this);
    }

    private void OnDisable()
    {
        // Unregister this icon when it's destroyed
        ScryingSystem.Instance?.UnregisterIconController(this);
    }

    private void Start()
    {
        revealableEntity = GetComponent<RevealableEntity>();
        enemy = GetComponent<Enemy>(); // It's okay if this is null

        CreateIcon();
    }

    private void CreateIcon()
    {
        if (scryingIconPrefab == null || iconAtlas == null) return;

        Transform anchor = transform.Find("RevealableIcon_Anchor"); // Use a consistent anchor name
        if (anchor == null) anchor = transform.Find("RevealIcon_Anchor"); // Fallback for old name
        if (anchor == null) anchor = transform; // Fallback to the object's root

        scryingIconInstance = Instantiate(scryingIconPrefab, anchor.position, anchor.rotation, anchor);

        Image iconImage = scryingIconInstance.GetComponentInChildren<Image>();

        // --- GENERIC ICON LOGIC ---
        bool isResistant = (enemy != null && enemy.Config.isResistantToScrying);

        if (isResistant)
        {
            iconImage.sprite = iconAtlas.distortedIcon;
        }
        else
        {
            RevealableType type = revealableEntity.Type;
            iconImage.sprite = iconAtlas.GetIcon(type);
            iconImage.color = iconAtlas.GetColor(type);
        }

        scryingIconInstance.SetActive(false);
    }

    public void ShowIcon() { if (scryingIconInstance != null) scryingIconInstance.SetActive(true); }
    public void HideIcon() { if (scryingIconInstance != null) scryingIconInstance.SetActive(false); }
    public void UpdateRotation(Quaternion cameraRotation)
    {
        if (scryingIconInstance != null && scryingIconInstance.activeSelf)
        {
            scryingIconInstance.transform.rotation = cameraRotation;
        }
    }
}