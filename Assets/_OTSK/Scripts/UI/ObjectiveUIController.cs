// ObjectiveUIController.cs
using DG.Tweening; // Or use a simple Lerp if you prefer
using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveUIController : MonoBehaviour
{
    [Header("Main Objective Elements")]
    [SerializeField] private CanvasGroup mainPanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI mainTitleText;
    [SerializeField] private TextMeshProUGUI mainCounterText;
    [SerializeField] private TextMeshProUGUI mainProgressText;

    [Header("Notification Elements")] 
    [SerializeField] private CanvasGroup notificationCanvasGroup;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Hint Elements")]
    [SerializeField] private GameObject objectiveHintObject;
    [SerializeField] private CanvasGroup hintCanvasGroup;

    [Header("Events")]
    [SerializeField] private ObjectiveProgressEvent onObjectiveProgressUpdated; 

    [Header("Animation Settings")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float displayTime = 4f;


    private Coroutine _mainDisplayCoroutine;
    private Coroutine _notificationCoroutine;
    private bool _hasShownFirstObjective = false;
    private ObjectiveSO _pendingFirstObjective = null;
    private bool _isObjectiveActive = false;

    private void Awake()
    {
        if (mainPanelCanvasGroup != null) mainPanelCanvasGroup.alpha = 0;
        if (notificationCanvasGroup != null) notificationCanvasGroup.alpha = 0;
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleMainObjectiveChanged;
            ObjectiveManager.Instance.OnSideObjectiveAdded += HandleSideObjectiveAdded; // --- NEW ---
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;

            if (!ObjectiveManager.Instance.IsRestoring)
            {
                HandleMainObjectiveChanged(ObjectiveManager.Instance.CurrentObjective);
            }
        }
        if (onObjectiveProgressUpdated != null) { onObjectiveProgressUpdated.OnEventRaised += HandleObjectiveProgress; }
        PlayerInputHandler.OnFirstGameplayInput += HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput += HandleShowObjectiveInput;
    }

    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged -= HandleMainObjectiveChanged;
            ObjectiveManager.Instance.OnSideObjectiveAdded -= HandleSideObjectiveAdded; // --- NEW ---
            ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }
        if (onObjectiveProgressUpdated != null) { onObjectiveProgressUpdated.OnEventRaised -= HandleObjectiveProgress; }
        PlayerInputHandler.OnFirstGameplayInput -= HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput -= HandleShowObjectiveInput;
    }


    private void HandleMainObjectiveChanged(ObjectiveSO newMainObjective)
    {
        if (!_hasShownFirstObjective)
        {
            _pendingFirstObjective = newMainObjective;
        }
        else
        {
            UpdateMainObjectiveDisplay(newMainObjective, true, true);
        }
    }

    // --- NEW METHOD --- This method is ONLY for SIDE objectives.
    private void HandleSideObjectiveAdded(ObjectiveSO newSideObjective)
    {
        if (newSideObjective == null) return;
        ShowNotification($"New Side Quest: {newSideObjective.objectiveTitle}");
    }

    private void HandleObjectiveProgress(ObjectiveProgressData data)
    {
        var currentMainObjective = ObjectiveManager.Instance.CurrentObjective;
        if (currentMainObjective == null || data.objectiveID != currentMainObjective.objectiveID) return;

        mainCounterText.text = data.counterLabel;
        mainProgressText.text = $"{data.currentProgress} / {data.requiredAmount}";

        if (_hasShownFirstObjective)
        {
            UpdateMainObjectiveDisplay(currentMainObjective, true, false);
        }
    }
    private void HandleLevelCompleted()
    {
        UpdateMainObjectiveDisplay(null, true, true, "All Objectives Complete!");
        //_isObjectiveActive = false;
        //if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
        //ShowObjective(null, true, "All Objectives Complete!");
    }

    private void ShowObjective(ObjectiveSO objective, bool shouldFadeOut, string overrideText = null)
    {
        if (objective == null && string.IsNullOrEmpty(overrideText))
        {
            mainPanelCanvasGroup.DOFade(0, fadeTime);
            return;
        }

        // This line correctly gets the title.
        string textToShow = overrideText ?? objective.objectiveTitle;

        if (_mainDisplayCoroutine != null) StopCoroutine(_mainDisplayCoroutine);

        // This line correctly calls the coroutine with the string and the bool.
        _mainDisplayCoroutine = StartCoroutine(ShowMainObjectiveCoroutine(textToShow, shouldFadeOut));
    }







    private IEnumerator ShowMainObjectiveCoroutine(string title, bool shouldFadeOut)
    {
        if (hintCanvasGroup != null) hintCanvasGroup.DOFade(0, fadeTime);
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayNewObjectiveSound();

        // This line correctly uses the 'title' parameter.
        mainTitleText.text = title;

        mainPanelCanvasGroup.DOFade(1, fadeTime);

        if (shouldFadeOut)
        {
            yield return new WaitForSeconds(displayTime);
            mainPanelCanvasGroup.DOFade(0, fadeTime);
            yield return new WaitForSeconds(fadeTime);
            if (objectiveHintObject != null && _isObjectiveActive)
            {
                objectiveHintObject.SetActive(true);
                if (hintCanvasGroup != null) hintCanvasGroup.DOFade(1, fadeTime);
            }
        }
    }

    public void ShowNotification(string text)
    {
        if (_notificationCoroutine != null) StopCoroutine(_notificationCoroutine);
        _notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(text));
    }

    private IEnumerator ShowNotificationCoroutine(string text)
    {
        if (notificationText != null) notificationText.text = text;
        if (notificationCanvasGroup != null)
        {
            // Use a sequence to ensure fades complete
            var sequence = DOTween.Sequence();
            sequence.Append(notificationCanvasGroup.DOFade(1, fadeTime));
            sequence.AppendInterval(displayTime);
            sequence.Append(notificationCanvasGroup.DOFade(0, fadeTime));
        }
        yield return null; // Coroutine needs to yield something
    }

    private void HandleFirstGameplayInput()
    {
        if (!_hasShownFirstObjective && _pendingFirstObjective != null)
        {
            _hasShownFirstObjective = true;
            UpdateMainObjectiveDisplay(_pendingFirstObjective, true, true);
            _pendingFirstObjective = null;
        }
    }

    private void UpdateMainObjectiveDisplay(ObjectiveSO objective, bool animate, bool shouldFadeOut, string overrideText = null)
    {
        if (_mainDisplayCoroutine != null) StopCoroutine(_mainDisplayCoroutine);

        if (objective == null && string.IsNullOrEmpty(overrideText))
        {
            mainPanelCanvasGroup.alpha = 0;
            if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
            return;
        }

        string title = overrideText ?? objective.objectiveTitle;
        var progressData = ObjectiveManager.Instance.GetCurrentProgressData();
        string label = "";
        string progress = "";

        if (progressData.HasValue && objective != null && progressData.Value.objectiveID == objective.objectiveID)
        {
            label = progressData.Value.counterLabel;
            progress = $"{progressData.Value.currentProgress} / {progressData.Value.requiredAmount}";
        }

        mainTitleText.text = title;
        mainCounterText.text = label;
        mainProgressText.text = progress;

        if (animate)
        {
            // --- THIS IS THE FIX ---
            // We now correctly pass BOTH the title and the shouldFadeOut flag to the coroutine.
            _mainDisplayCoroutine = StartCoroutine(ShowMainObjectiveCoroutine(title, shouldFadeOut));
        }
    }


    private void HandleShowObjectiveInput()
    {
        // If the panel is already showing, don't do anything
        if (_mainDisplayCoroutine != null && mainPanelCanvasGroup.alpha > 0) return;

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