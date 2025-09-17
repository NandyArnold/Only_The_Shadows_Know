// MapEdgeIndicatorController.cs - Final Version
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapEdgeIndicatorController : MonoBehaviour
{
    [Header("Asset References")]
    [SerializeField] private TacticalIconAtlasSO iconAtlas;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Scene References")]
    [SerializeField] private RectTransform mapPanelRect;

    [Header("Settings")]
    [SerializeField] private int poolSize = 5;
    //[SerializeField] private float borderWidth = 30f;

    [Header("Appearance")]
    [Tooltip("How far inside the minimap's edge the arrow should be. Increase to pull it in.")]
    [SerializeField] private float radiusOffset = 30f;

    private List<Image> arrowPool = new List<Image>();
    private Camera scryingRenderCamera;
    private Transform playerTransform;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;
        }
    }

    private void HandlePlayerRegistered(PlayerController player)
    {
        playerTransform = player.transform;
    }

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, mapPanelRect);
            arrowObj.SetActive(false);
            arrowPool.Add(arrowObj.GetComponent<Image>());
        }
    }

    void LateUpdate()
    {
        if (scryingRenderCamera == null)
        {
            if (ScryingSystem.Instance != null)
            {
                scryingRenderCamera = ScryingSystem.Instance.ScryingRenderCamera;
            }
            if (scryingRenderCamera == null) return;
        }

        if (playerTransform == null || !scryingRenderCamera.gameObject.activeInHierarchy)
        {
            foreach (var arrow in arrowPool) if (arrow.gameObject.activeSelf) arrow.gameObject.SetActive(false);
            return;
        }

        // At the start of the frame, hide all arrows. We'll show them again if needed.
        foreach (var arrow in arrowPool)
        {
            arrow.gameObject.SetActive(false);
        }

        int activeArrowIndex = 0;
        var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
        if (activeObjectives == null) return;

        //Debug.Log($"[PAUSING EXECUTION] Found {activeObjectives.Count()} objective(s). Now inspect the ObjectiveManager.", ObjectiveManager.Instance);
        //Debug.Break();

        //Debug.Log($"[IndicatorController] LateUpdate running. Found {activeObjectives.Count()} active objective(s).");

        foreach (var objectiveInstance in activeObjectives)
        {
            if (activeArrowIndex >= poolSize) break; // Stop if we run out of arrows in our pool
            ProcessObjective(objectiveInstance.SourceSO, ref activeArrowIndex);
        }

        // You could add a loop here later to process side objectives as well

        // --- THIS IS THE MAIN FIX ---
        // 1. Get the complete list of active objectives from the manager.
        //var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();

        //// 2. Loop through every active objective.
        //foreach (var objectiveInstance in activeObjectives)
        //{
        //    // The manager gives us an ObjectiveInstance, which contains the ObjectiveSO data.
        //    // We pass that data to our processing method.
        //    ProcessObjective(objectiveInstance.SourceSO, ref activeArrowIndex);
        //}
    }

    private void ProcessObjective(ObjectiveSO objective, ref int arrowIndex)
    {
        Transform targetTransform = GetTargetTransformForObjective(objective);

        // Now, proceed with the original logic using the found transform.
        if (targetTransform == null || arrowIndex >= poolSize) return;

        Vector3 objectivePos = targetTransform.position;
        Vector3 viewportPos = scryingRenderCamera.WorldToViewportPoint(objectivePos);

        bool isBehind = viewportPos.z < 0;
        if (isBehind)
        {
            viewportPos = -viewportPos;
        }

        bool isOffScreen = isBehind || (viewportPos.x < 0.02f || viewportPos.x > 0.98f || viewportPos.y < 0.02f || viewportPos.y > 0.98f);

        //Debug.Log($"[IndicatorController] Processing '{objective.objectiveTitle}'. Is target off-screen? {isOffScreen}");

        if (isOffScreen)
        {
            Image arrow = arrowPool[arrowIndex];
            arrow.gameObject.SetActive(true);
            arrowIndex++;

            arrow.sprite = GetSpriteForObjectiveType(objective.objectiveType);

            Vector3 screenPos = scryingRenderCamera.ViewportToScreenPoint(viewportPos);
            Vector2 mapCenter = mapPanelRect.position;
            Vector2 direction = new Vector2(screenPos.x - mapCenter.x, screenPos.y - mapCenter.y);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            arrow.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

            // Get the radius of the map panel directly from its rectangle properties.
            // We divide by 2 for the radius and account for canvas scaling.
            float radius = (mapPanelRect.rect.width / 2f) * mapPanelRect.lossyScale.x;

            // Subtract the border width to bring the arrow slightly inside the edge.
            radius -= radiusOffset;

            // The final position is the center of the map panel plus the direction vector clamped to the new radius.
            arrow.rectTransform.position = mapPanelRect.position + (Vector3)direction.normalized * radius;
        }
    }

    private Sprite GetSpriteForObjectiveType(ObjectiveType type)
    {
        switch (type)
        {
            case ObjectiveType.MainObjective: return iconAtlas.mainObjectiveArrow;
            case ObjectiveType.SideObjective: return iconAtlas.sideObjectiveArrow;
            case ObjectiveType.ExitPoint: return iconAtlas.exitPointArrow;
            default: return iconAtlas.defaultObjectiveArrow;
        }
    }

    private Transform GetTargetTransformForObjective(ObjectiveSO objective)
    {
        //Debug.Log($"[IndicatorController] Searching for transform for objective: '{objective.objectiveTitle}'");
        Transform foundTransform = null;

        // Case 1: The objective has a specific, static location ID.
        if (!string.IsNullOrEmpty(objective.targetLocationID))
        {
            var targets = ObjectiveTargetRegistry.Instance.FindTargetsByID(objective.targetLocationID);
            if (targets != null && targets.Count > 0)
            {
                foundTransform = targets[0];
            }
        }

        // Case 2: It's a dynamic goal, like killing enemies or destroying objects.
        else if (objective.goalType == ObjectiveGoalType.Kill && objective.killGoal.requiredEnemyIDs.Count > 0)
        {
            string targetID = objective.killGoal.requiredEnemyIDs[0];
            var potentialTargets = ObjectiveTargetRegistry.Instance.FindTargetsByID(targetID);
            // Find the closest target from the list...
            foundTransform = GetClosestTransform(potentialTargets);
        }
        else if (objective.goalType == ObjectiveGoalType.Destroy && objective.destroyGoal.requiredDestructibleIDs.Count > 0)
        {
            string targetID = objective.destroyGoal.requiredDestructibleIDs[0];
            var potentialTargets = ObjectiveTargetRegistry.Instance.FindTargetsByID(targetID);
            // Find the closest target from the list...
            foundTransform = GetClosestTransform(potentialTargets);
        }

        // --- ADDED LOGGING ---
        if (foundTransform != null)
        {
            //Debug.Log($"<color=green>[IndicatorController] SUCCESS! Found transform: '{foundTransform.name}' for objective '{objective.objectiveTitle}'</color>", foundTransform.gameObject);
        }
        else
        {
            //Debug.LogWarning($"<color=orange>[IndicatorController] FAILED to find any transform for objective '{objective.objectiveTitle}'.</color>");
        }
        return foundTransform;
    }

    private Transform GetClosestTransform(List<Transform> targets)
    {
        if (targets == null || targets.Count == 0) return null;

        Transform player = GameManager.Instance.Player.transform;
        Transform closestTarget = null;
        float minDistanceSqr = float.MaxValue;

        foreach (var target in targets)
        {
            if (target == null) continue;
            float distanceSqr = (player.position - target.position).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closestTarget = target;
            }
        }
        return closestTarget;
    }
}