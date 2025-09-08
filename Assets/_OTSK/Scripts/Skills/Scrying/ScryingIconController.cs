// ScryingIconController.cs - Generic Version
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RevealableEntity))]
public class ScryingIconController : MonoBehaviour
{
    public GameObject scryingIconInstance { get; private set; }
    [Header("Configuration")]
    //[SerializeField] private GameObject scryingIconPrefab;
    //[SerializeField] private TacticalIconAtlasSO iconAtlas;
    [Tooltip("The fixed Y-axis height in world space for the icon to appear.")]
    [SerializeField] private float iconWorldHeight = 120f;
    public GameObject IconInstance { get; private set; }
    public RectTransform IconImageRectTransform { get; private set; }

    public Transform TargetTransform => transform;

    private RevealableEntity revealableEntity;
    private Enemy enemy; // Optional, for checking resistance
    private Objective objective;
    private Canvas iconCanvas;
    public bool IsObjective => objective != null;

    private void Awake()
    {
        revealableEntity = GetComponent<RevealableEntity>();
        enemy = GetComponent<Enemy>();
        objective = GetComponent<Objective>();
        //CreateIcon();
    }

    private void OnEnable()
    {
        // It's good practice to register the icon. Consider uncommenting this
        // and the corresponding code in ScryingSystem for better performance.
        ScryingSystem.Instance?.RegisterIconController(this);
    }

    private void OnDisable()
    {
        // Always unregister when disabled or destroyed.
        ScryingSystem.Instance?.UnregisterIconController(this);
        if (IconInstance != null)
        {
            Destroy(IconInstance);
        }
    }

    public void CreateIcon(GameObject scryingIconPrefab, TacticalIconAtlasSO iconAtlas)
    {
        // If an icon already exists (e.g., from a previous attempt), destroy it first.
        if (IconInstance != null) { Destroy(IconInstance); }

        // Guard clause using the new arguments.
        if (scryingIconPrefab == null || iconAtlas == null) return;

        // 1. Calculate the spawn position at our desired height.
        Vector3 ownerPosition = transform.position;
        Vector3 spawnPosition = new Vector3(ownerPosition.x, iconWorldHeight, ownerPosition.z);

        // 2. Instantiate the icon at the new position with NO parent.
        IconInstance = Instantiate(scryingIconPrefab, spawnPosition, Quaternion.identity, null);

        iconCanvas = IconInstance.GetComponent<Canvas>();
        // The rest of the setup logic remains the same.
        Image iconImage = IconInstance.GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            IconImageRectTransform = iconImage.rectTransform;
        }

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

    public void SetSortOrder(int order)
    {
        if (iconCanvas != null)
        {
            iconCanvas.sortingOrder = order;
        }
    }
    public void ShowIcon() { if (IconInstance != null) IconInstance.SetActive(true); }
    public void HideIcon() { if (IconInstance != null) IconInstance.SetActive(false); }

}