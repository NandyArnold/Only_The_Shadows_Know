// ScryingIconController.cs - Corrected Definitive Version
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RevealableEntity))]
public class ScryingIconController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The fixed Y-axis height in world space for the icon to appear.")]
    [SerializeField] private float iconWorldHeight = 120f;

    // --- Public Properties for the Scrying System ---
    public GameObject IconInstance { get; private set; }
    public RectTransform IconImageRectTransform { get; private set; }
    public RectTransform FacingIndicatorTransform { get; private set; }

    public Transform TargetTransform => transform;
    public bool IsObjective => objective != null;
    public bool IsOwnerDead => enemyHealth != null && enemyHealth.IsDead;

    // --- Private Component References ---
    //private RevealableEntity revealableEntity;
    //private Enemy enemy;
    private Objective objective;
    private EnemyHealth enemyHealth;
    private Canvas iconCanvas;

    public Enemy enemy { get; private set; }
    public RevealableEntity revealableEntity { get; private set; }

    // --- Add this new property to easily identify the player ---
    public bool IsPlayer => revealableEntity != null && revealableEntity.Type == RevealableType.Player;



    private void Awake()
    {
        revealableEntity = GetComponent<RevealableEntity>();
        enemy = GetComponent<Enemy>();
        objective = GetComponent<Objective>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied += HandleOwnerDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= HandleOwnerDied;
        }
    }

    private void OnDestroy()
    {
        if (IconInstance != null)
        {
            Destroy(IconInstance);
        }
    }

    private void HandleOwnerDied(bool isSilentKill)
    {
        if (ScryingSystem.Instance != null)
        {
            ScryingSystem.Instance.ReportDeath(this);
        }
    }

    public void CreateIcon(GameObject iconPrefab, TacticalIconAtlasSO atlas)
    {
        if (IconInstance != null) { Destroy(IconInstance); }
        if (iconPrefab == null || atlas == null) return;

        Vector3 ownerPosition = transform.position;
        Vector3 spawnPosition = new Vector3(ownerPosition.x, iconWorldHeight, ownerPosition.z);
        IconInstance = Instantiate(iconPrefab, spawnPosition, Quaternion.identity, null);

        iconCanvas = IconInstance.GetComponent<Canvas>();
        Image iconImage = IconInstance.GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            IconImageRectTransform = iconImage.rectTransform;
        }

        // Find the FacingIndicator child and store its transform.
        Transform indicator = IconInstance.transform.Find("FacingIndicator");
        
        if (indicator != null)
        {
            FacingIndicatorTransform = indicator.GetComponent<RectTransform>();
        }

        // --- THIS IS THE FULLY RESTORED AND CORRECTED SPRITE LOGIC ---
        bool isResistant = (enemy != null && enemy.Config.isResistantToScrying);
        if (isResistant)
        {
            iconImage.sprite = atlas.distortedIcon;
        }
        else if (enemy != null)
        {
            iconImage.sprite = atlas.GetIcon(enemy.Config.enemyType);
            iconImage.color = atlas.GetColor(enemy.Config.enemyType);
        }
        else if (revealableEntity != null)
        {
            iconImage.sprite = atlas.GetIcon(revealableEntity.Type);
            iconImage.color = atlas.GetColor(revealableEntity.Type);
        }
        else
        {
            iconImage.sprite = atlas.defaultIcon;
        }
        // -----------------------------------------------------------------

        IconInstance.SetActive(false);
    }

    public void ShowIcon() { if (IconInstance != null) IconInstance.SetActive(true); }
    public void HideIcon() { if (IconInstance != null) IconInstance.SetActive(false); }
    public void SetSortOrder(int order) { if (iconCanvas != null) iconCanvas.sortingOrder = order; }
}