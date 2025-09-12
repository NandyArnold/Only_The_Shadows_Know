// ObjectiveUIController.cs
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening; // Or use a simple Lerp if you prefer

public class ObjectiveUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject objectiveHintObject;
    [SerializeField] private CanvasGroup hintCanvasGroup;

    [Header("Events")]
    [SerializeField] private ObjectiveProgressEvent onObjectiveProgressUpdated; 

    [Header("Animation Settings")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float displayTime = 4f;


    private Coroutine _displayCoroutine;
    private bool _hasShownFirstObjective = false;
    private ObjectiveSO _pendingFirstObjective = null;
    private bool _isObjectiveActive = false;

    private void Awake()
    {
        // Ensure the panel is invisible at the start.
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0;
        }
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
    }

    private void OnEnable()
    {
        Debug.Log("<color=lime>--- UI CONTROLLER: OnEnable CALLED ---</color>");

        if (ObjectiveManager.Instance != null)
        {
            Debug.Log($"<color=lime>--- UI CONTROLLER: Checking IsRestoring flag. Value is: {ObjectiveManager.Instance.IsRestoring}</color>");
            if (!ObjectiveManager.Instance.IsRestoring)
                ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;

            //HandleNewObjective(ObjectiveManager.Instance.CurrentObjective);

            if (!ObjectiveManager.Instance.IsRestoring)
            {
                Debug.Log($"<color=lime>--- UI CONTROLLER: NOT restoring. Syncing with manager. Current objective is:" +
                    $" '{ObjectiveManager.Instance.CurrentObjective?.objectiveDescription ?? "None"}'</color>");
                HandleNewObjective(ObjectiveManager.Instance.CurrentObjective);
                var initialProgress = ObjectiveManager.Instance.GetCurrentProgressData();
                if (initialProgress.HasValue)
                {
                    HandleObjectiveProgress(initialProgress.Value);
                }
            }
            else
            {
                Debug.Log("<color=lime>--- UI CONTROLLER: Manager IS restoring. Skipping initial sync.</color>");
            }


        }
        if (onObjectiveProgressUpdated != null)
        {
            onObjectiveProgressUpdated.OnEventRaised += HandleObjectiveProgress;
            //var initialProgress = ObjectiveManager.Instance.GetCurrentProgressData();
            //if (initialProgress.HasValue)
            //{
            //    HandleObjectiveProgress(initialProgress.Value);
            //}
        }

        PlayerInputHandler.OnFirstGameplayInput += HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput += HandleShowObjectiveInput;
    }




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
  

    private void HandleNewObjective(ObjectiveSO newObjective)
    {
        if (newObjective != null && newObjective.isHidden)
        {
            _isObjectiveActive = true;
            return; // Exit without showing the UI
        }

        Debug.Log($"<color=green>--- UI CONTROLLER: HandleNewObjective EVENT RECEIVED. Objective is:" +
            $" '{newObjective?.objectiveDescription ?? "None"}' ---</color>");
        _isObjectiveActive = newObjective != null;
        if (!_isObjectiveActive)
        {
            if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
            ShowObjective(null, true);
            return;
        }

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
            ShowObjective(newObjective, true);
        }
    }

    private void HandleObjectiveProgress(ObjectiveProgressData data)
    {
        Debug.Log($"<color=green>[ObjectiveUIController]</color> RECEIVED progress event. Updating UI. Label:" +
            $" '{data.counterLabel}', Progress: {data.currentProgress}/{data.requiredAmount}.");
 
        counterText.text = data.counterLabel;
        progressText.text = $"{data.currentProgress} / {data.requiredAmount}";

        if (_hasShownFirstObjective)
        {
            ShowObjective(ObjectiveManager.Instance.CurrentObjective, false);
        }

    

    }

    private void HandleLevelCompleted()
    {
        _isObjectiveActive = false;
        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
        ShowObjective(null, true, "All Objectives Complete!");
    }

    private void ShowObjective(ObjectiveSO objective, bool shouldFadeOut, string overrideText = null)
    {
        if (objective == null && string.IsNullOrEmpty(overrideText))
        {
            panelCanvasGroup.DOFade(0, fadeTime);
            return;
        }
        string textToShow = overrideText ?? objective.objectiveTitle;
        if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
        _displayCoroutine = StartCoroutine(ShowObjectiveCoroutine(textToShow, shouldFadeOut));
    }

    private IEnumerator ShowObjectiveCoroutine(string title, bool shouldFadeOut)
    {
        // Fade hint out
        if (hintCanvasGroup != null) hintCanvasGroup.DOFade(0, fadeTime);

        // Play sound and set text
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayNewObjectiveSound();
        titleText.text = title;

        // Fade main panel in
        panelCanvasGroup.DOFade(1, fadeTime);

        // Only do the timed fade out if requested
        if (shouldFadeOut)
        {
            yield return new WaitForSeconds(displayTime);
            panelCanvasGroup.DOFade(0, fadeTime);

            // Wait for the fade out to finish
            yield return new WaitForSeconds(fadeTime);

            // Now fade the hint back in
            if (objectiveHintObject != null && _isObjectiveActive)
            {
                objectiveHintObject.SetActive(true);
                if (hintCanvasGroup != null) hintCanvasGroup.DOFade(1, fadeTime);
            }
        }
    }

    private void HandleFirstGameplayInput()
    {

        if (_pendingFirstObjective == null) _pendingFirstObjective = ObjectiveManager.Instance.CurrentObjective;

        if (!_hasShownFirstObjective && _pendingFirstObjective != null)
        {
            _hasShownFirstObjective = true;
            ShowObjective(_pendingFirstObjective, true);
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

            ShowObjective(currentObjective, true);
            // Re-run the same fade coroutine we already have
            //if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
            //_displayCoroutine = StartCoroutine(ShowObjectiveCoroutine(currentObjective.objectiveDescription));
        }
    }
}