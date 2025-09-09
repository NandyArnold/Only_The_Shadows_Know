// ObjectiveUIController.cs
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening; // Or use a simple Lerp if you prefer

public class ObjectiveUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Events")]
    [SerializeField] private ObjectiveProgressEvent onObjectiveProgressUpdated; // Assign your new event asset

    [Header("Animation Settings")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float displayTime = 4f;

    private Coroutine _displayCoroutine;

    private bool _hasShownFirstObjective = false;
    private ObjectiveSO _pendingFirstObjective = null;

    private void Awake()
    {
        // Ensure the panel is invisible at the start.
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0;
        }
    }

    private void OnEnable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        if (onObjectiveProgressUpdated != null)
        {
            onObjectiveProgressUpdated.OnEventRaised += HandleObjectiveProgress;
            Debug.Log("<color=lime>[ObjectiveUIController]</color> Successfully SUBSCRIBED to progress event.");

            // Keep the sync logic!
            var initialProgress = ObjectiveManager.Instance.GetCurrentProgressData();
            if (initialProgress.HasValue)
            {
                HandleObjectiveProgress(initialProgress.Value);
            }
        }
        else
        {
            Debug.LogWarning("[ObjectiveUIController] The 'On Objective Progress Updated' event is NULL in the Inspector.");
        }

        PlayerInputHandler.OnFirstGameplayInput += HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput += HandleShowObjectiveInput;
    }

    //private void OnEnable()
    //{
    //    // Subscribe to the manager that announces new objectives
    //    if (ObjectiveManager.Instance != null)
    //    {
    //        ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleNewObjective;
    //        ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;

    //        HandleNewObjective(ObjectiveManager.Instance.CurrentObjective);
    //    }
    //    // Subscribe to the event that announces progress updates
    //    if (onObjectiveProgressUpdated != null)
    //    {
    //        onObjectiveProgressUpdated.OnEventRaised += HandleObjectiveProgress;
    //        Debug.Log("<color=lime>[ObjectiveUIController]</color> Successfully SUBSCRIBED to OnObjectiveProgressUpdated event.");
    //    }
    //    else
    //    {
    //        // --- ADD THIS LOG ---
    //        Debug.LogWarning("<color=red>[ObjectiveUIController]</color> FAILED to subscribe to progress event because the event asset is NULL in the Inspector.");
    //    }
    //    PlayerInputHandler.OnFirstGameplayInput += HandleFirstGameplayInput;
    //    PlayerInputHandler.OnShowObjectiveInput += HandleShowObjectiveInput;
    //}


    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged -= HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }

        if (onObjectiveProgressUpdated != null)
        {
            onObjectiveProgressUpdated.OnEventRaised -= HandleObjectiveProgress;
        }

        PlayerInputHandler.OnFirstGameplayInput -= HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput -= HandleShowObjectiveInput;
    }
    //private void OnDisable()
    //{
    //    if (ObjectiveManager.Instance != null)
    //    {
    //        ObjectiveManager.Instance.OnCurrentObjectiveChanged -= HandleNewObjective;
    //        ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
    //    }
    //    if (onObjectiveProgressUpdated != null)
    //    {
    //        onObjectiveProgressUpdated.OnEventRaised -= HandleObjectiveProgress;
    //    }
    //    PlayerInputHandler.OnFirstGameplayInput -= HandleFirstGameplayInput;
    //    PlayerInputHandler.OnShowObjectiveInput -= HandleShowObjectiveInput;
    //}

    private void HandleNewObjective(ObjectiveSO newObjective)
    {
        if (newObjective != null)
        {
            Debug.Log($"<color=green>[ObjectiveUIController]</color> Received new objective to display: '{newObjective.objectiveDescription}'");
        }
        else
        {
            Debug.Log("<color=green>[ObjectiveUIController]</color> Received a null objective (end of chain).");
        }
        // If this is the very first objective of the scene, don't show it yet.
        if (!_hasShownFirstObjective)
        {
            _pendingFirstObjective = newObjective;
            // Clear any old progress text, just in case
            counterText.text = "";
            progressText.text = "";
        }
        else
        {
            // For all subsequent objectives, show them immediately.
            ShowObjective(newObjective);
        }
    }

    private void HandleObjectiveProgress(ObjectiveProgressData data)
    {
        Debug.Log($"<color=green>[ObjectiveUIController]</color> RECEIVED progress event. Updating UI. Label:" +
            $" '{data.counterLabel}', Progress: {data.currentProgress}/{data.requiredAmount}.");
        // Update the UI with the data received from the event
        if (counterText == null || progressText == null)
        {
            Debug.LogError("[ObjectiveUIController] The Counter Text or Progress Text reference is NULL in the Inspector!", this.gameObject);
            return;
        }
        counterText.text = data.counterLabel;
        progressText.text = $"{data.currentProgress} / {data.requiredAmount}";
    }

    private void HandleLevelCompleted()
    {
        ShowObjective(null, "All Objectives Complete!");
    }

    private void ShowObjective(ObjectiveSO objective, string overrideText = null)
    {
        if (objective == null && string.IsNullOrEmpty(overrideText))
        {
            panelCanvasGroup.DOFade(0, fadeTime);
            return;
        }

        string textToShow = overrideText ?? objective.objectiveDescription;

        if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
        _displayCoroutine = StartCoroutine(ShowObjectiveCoroutine(textToShow));
    }

    private IEnumerator ShowObjectiveCoroutine(string description)
    {
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayNewObjectiveSound();
        descriptionText.text = description;
        panelCanvasGroup.DOFade(1, fadeTime);
        yield return new WaitForSeconds(displayTime);
        panelCanvasGroup.DOFade(0, fadeTime);
    }

    private void HandleFirstGameplayInput()
    {

        if (_pendingFirstObjective == null) _pendingFirstObjective = ObjectiveManager.Instance.CurrentObjective;

        if (!_hasShownFirstObjective && _pendingFirstObjective != null)
        {
            _hasShownFirstObjective = true;
            ShowObjective(_pendingFirstObjective);
            _pendingFirstObjective = null; // Clear it so it doesn't show again
        }
    }

    private void HandleShowObjectiveInput()
    {
        // If the panel is already showing, don't do anything
        if (_displayCoroutine != null && panelCanvasGroup.alpha > 0) return;

        // Get the current objective from the manager
        var currentObjective = ObjectiveManager.Instance.CurrentObjective;
        if (currentObjective != null)
        {
            if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayToggleSound();

            // Re-run the same fade coroutine we already have
            if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
            _displayCoroutine = StartCoroutine(ShowObjectiveCoroutine(currentObjective.objectiveDescription));
        }
    }
}