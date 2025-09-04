// ScryingIconController.cs - Generic Version
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RevealableEntity))]
public class ScryingIconController : MonoBehaviour
{
    public GameObject scryingIconInstance { get; private set; }
    [Header("Configuration")]
    [SerializeField] private GameObject scryingIconPrefab;
    [SerializeField] private TacticalIconAtlasSO iconAtlas;
    public GameObject IconInstance { get; private set; }
    public RectTransform IconImageRectTransform { get; private set; }

    private RevealableEntity revealableEntity;
    private Enemy enemy; // Optional, for checking resistance

    private void Awake()
    {
        CreateIcon();
    }

    //private void OnEnable()
    //{
    //    // Register this icon with the central system
    //    ScryingSystem.Instance?.RegisterIconController(this);
    //}

    //private void OnDisable()
    //{
    //    // Unregister this icon when it's destroyed
    //    ScryingSystem.Instance?.UnregisterIconController(this);
    //    if (IconInstance != null)
    //    {
    //        Destroy(IconInstance);
    //    }
    //}

    //private void Start()
    //{
    //    revealableEntity = GetComponent<RevealableEntity>();
    //    enemy = GetComponent<Enemy>(); // It's okay if this is null

       
    //}

    private void CreateIcon()
    {
        if (scryingIconPrefab == null || iconAtlas == null) return;

        Transform anchor = transform.Find("RevealableIcon_Anchor") ?? transform;

        // Instantiate the icon and store the reference
        IconInstance = Instantiate(scryingIconPrefab, anchor.position, anchor.rotation, anchor);

        Image iconImage = IconInstance.GetComponentInChildren<Image>();

        if (iconImage != null)
        {
            IconImageRectTransform = iconImage.rectTransform;
        }

        // --- Set Sprite Logic ---
        bool isResistant = (enemy != null && enemy.Config.isResistantToScrying);
        if (isResistant)
        {
            iconImage.sprite = iconAtlas.distortedIcon;
        }
        else if (enemy != null)
        {
            iconImage.sprite = iconAtlas.GetIcon(enemy.Config.enemyType);
            iconImage.color = iconAtlas.GetColor(enemy.Config.enemyType);
        }
        else if (revealableEntity != null)
        {
            iconImage.sprite = iconAtlas.GetIcon(revealableEntity.Type);
            iconImage.color = iconAtlas.GetColor(revealableEntity.Type);
        }
        else
        {
            iconImage.sprite = iconAtlas.defaultIcon;
        }

        IconInstance.SetActive(false);

    }

    public void ShowIcon() { if (scryingIconInstance != null) scryingIconInstance.SetActive(true); }
    public void HideIcon() { if (scryingIconInstance != null) scryingIconInstance.SetActive(false); }
    
}