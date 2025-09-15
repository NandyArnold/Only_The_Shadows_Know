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
        // Ensure the panel is invisible at the start.
        if (mainPanelCanvasGroup != null)
        {
            mainPanelCanvasGroup.alpha = 0;
        }
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        if (notificationCanvasGroup != null) notificationCanvasGroup.alpha = 0;
        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnCurrentObjectiveChanged += HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            if (!ObjectiveManager.Instance.IsRestoring)
            {
                HandleNewObjective(ObjectiveManager.Instance.CurrentObjective);
                var initialProgress = ObjectiveManager.Instance.GetCurrentProgressData();
                if (initialProgress.HasValue) { HandleObjectiveProgress(initialProgress.Value); }
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
            ObjectiveManager.Instance.OnCurrentObjectiveChanged -= HandleNewObjective;
            ObjectiveManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }
        if (onObjectiveProgressUpdated != null) { onObjectiveProgressUpdated.OnEventRaised -= HandleObjectiveProgress; }
        PlayerInputHandler.OnFirstGameplayInput -= HandleFirstGameplayInput;
        PlayerInputHandler.OnShowObjectiveInput -= HandleShowObjectiveInput;
    }


    private void HandleNewObjective(ObjectiveSO newObjective)
    {
        if (newObjective != null && newObjective.isHidden)
        {
            // Silently track hidden objectives, don't show any UI for them yet.
            _isObjectiveActive = true;
            return;
        }

        _isObjectiveActive = newObjective != null;
        if (!_isObjectiveActive)
        {
            if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
            ShowObjective(null, true);
            return;
        }

        // Check the objective type to decide what to do
        if (newObjective.objectiveType == ObjectiveType.MainObjective)
        {
            // It's a main quest, use the main panel logic
            if (!_hasShownFirstObjective)
            {
                _pendingFirstObjective = newObjective;
                mainCounterText.text = "";
                mainProgressText.text = "";
            }
            else
            {
                ShowObjective(newObjective, true);
            }
        }
        else // It's a SideObjective, HiddenItem, etc.
        {
            string notificationPrefix = "New Objective:"; // Default text

            // Customize the prefix based on the type
            switch (newObjective.objectiveType)
            {
                case ObjectiveType.SideObjective:
                    notificationPrefix = "New Side Objective:";
                    break;
                case ObjectiveType.HiddenItem:
                    notificationPrefix = "Hidden Objective Discovered!:";
                    break;
            }

            // Show the customized notification
            ShowNotification($"{notificationPrefix} {newObjective.objectiveTitle}");
        }
    
    }

    private void HandleObjectiveProgress(ObjectiveProgressData data)
    {
        mainCounterText.text = data.counterLabel;
        mainProgressText.text = $"{data.currentProgress} / {data.requiredAmount}";

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
            mainPanelCanvasGroup.DOFade(0, fadeTime);
            return;
        }
        string textToShow = overrideText ?? objective.objectiveTitle;
        if (_mainDisplayCoroutine != null) StopCoroutine(_mainDisplayCoroutine);
        _mainDisplayCoroutine = StartCoroutine(ShowMainObjectiveCoroutine(textToShow, shouldFadeOut));
    }


    




    private IEnumerator ShowMainObjectiveCoroutine(string title, bool shouldFadeOut)
    {
        if (hintCanvasGroup != null) hintCanvasGroup.DOFade(0, fadeTime);
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayNewObjectiveSound();
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