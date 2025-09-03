// TacticalMapIconController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TacticalMapIconController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TacticalIconAtlasSO iconAtlas;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private int initialPoolSize = 50;

    [Header("UI References")]
    [SerializeField] private RectTransform minimapIconContainer;
    [SerializeField] private RectTransform fullMapIconContainer;

    private List<Image> iconPool = new List<Image>();
    private Camera scryingRenderCamera;

    private void Start()
    {
        // Get the scrying camera once the system is ready
        StartCoroutine(GetScryingCameraRoutine());

        // Create a pool of icons to use so we don't instantiate during gameplay
        InitializeIconPool();
    }

    private System.Collections.IEnumerator GetScryingCameraRoutine()
    {
        // Wait for the scrying system to initialize and get its camera reference
        yield return new WaitUntil(() => ScryingSystem.Instance != null && ScryingSystem.Instance.ScryingRenderCamera != null);
        scryingRenderCamera = ScryingSystem.Instance.ScryingRenderCamera;
    }

    private void InitializeIconPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject iconObj = Instantiate(iconPrefab, fullMapIconContainer); // Start on the full map
            iconObj.SetActive(false);
            iconPool.Add(iconObj.GetComponent<Image>());
        }
    }

    // This is the main loop that updates the icons every frame
    private void LateUpdate()
    {
        if (scryingRenderCamera == null || !ScryingSystem.Instance.IsScryingDeployed)
        {
            // If the camera isn't ready or scrying isn't active, do nothing.
            // You might want to also hide all icons here.
            return;
        }

        // Determine which container is active for the icons
        bool isFullMapActive = fullMapIconContainer.gameObject.activeInHierarchy;
        RectTransform activeContainer = isFullMapActive ? fullMapIconContainer : minimapIconContainer;

        int poolIndex = 0; // Keep track of which icon from the pool we're using

        // --- Draw Enemy Icons ---
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.GetActiveEnemies())
            {
                if (poolIndex >= iconPool.Count) break; // Stop if we run out of icons in the pool

                Image iconImage = iconPool[poolIndex];
                UpdateIcon(iconImage, activeContainer, enemy.transform.position, enemy.Config.enemyType, enemy.Config.isResistantToScrying);
                poolIndex++;
            }
        }

        // --- Draw Objective Icon ---
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveSO currentObjective = ObjectiveManager.Instance.GetCurrentObjective();
            if (currentObjective != null && currentObjective.objectiveLocation != null)
            {
                if (poolIndex < iconPool.Count)
                {
                    Image iconImage = iconPool[poolIndex];
                    UpdateIcon(iconImage, activeContainer, currentObjective.objectiveLocation.position, currentObjective.objectiveType, false); // Objectives are not resistant
                    poolIndex++;
                }
            }
        }

        // --- Disable Unused Icons ---
        for (int i = poolIndex; i < iconPool.Count; i++)
        {
            if (iconPool[i].gameObject.activeSelf)
            {
                iconPool[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates a single icon's properties (sprite, position, color, etc.).
    /// </summary>
    private void UpdateIcon(Image icon, RectTransform container, Vector3 worldPosition, object type, bool isResistant)
    {
        // --- Blind Spot Check ---
        // ToDo: Check if worldPosition is inside a "ScryingBlindSpot" volume. If so, return without activating the icon.
        // if (Physics.CheckSphere(worldPosition, 1f, blindSpotLayerMask)) {
        //     icon.gameObject.SetActive(false);
        //     return;
        // }

        // --- Convert World Position to UI Position ---
        Vector3 screenPosition = scryingRenderCamera.WorldToScreenPoint(worldPosition);

        // Only process icons that are in front of the camera
        if (screenPosition.z < 0)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        // Check if the icon is within the bounds of the container
        if (RectTransformUtility.RectangleContainsScreenPoint(container, screenPosition, null))
        {
            icon.gameObject.SetActive(true);
            icon.rectTransform.SetParent(container, false);
            icon.rectTransform.position = screenPosition;

            // --- Set the Icon Sprite and Color using our new Atlas methods ---
            Sprite spriteToShow = null;
            Color colorToApply = Color.white;

            if (isResistant)
            {
                spriteToShow = iconAtlas.distortedIcon;
            }
            else if (type is EnemyType enemyType)
            {
                spriteToShow = iconAtlas.GetIcon(enemyType);
                colorToApply = iconAtlas.GetColor(enemyType);
            }
            else if (type is ObjectiveType objectiveType)
            {
                spriteToShow = iconAtlas.GetIcon(objectiveType);
                colorToApply = iconAtlas.GetColor(objectiveType);
            }

            icon.sprite = spriteToShow;
            icon.color = colorToApply;
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Converts a world position into a local position within a UI RectTransform.
    /// </summary>
    private Vector2 WorldToMapPosition(Vector3 worldPos, RectTransform mapContainer)
    {
        Vector2 viewportPoint = scryingRenderCamera.WorldToViewportPoint(worldPos);
        Vector2 mapSize = mapContainer.rect.size;

        // Convert viewport point (0 to 1) to a position relative to the container's pivot
        return new Vector2(
            (viewportPoint.x * mapSize.x) + mapContainer.rect.xMin,
            (viewportPoint.y * mapSize.y) + mapContainer.rect.yMin
        );
    }
}