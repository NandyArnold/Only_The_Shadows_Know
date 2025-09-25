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
    private Sequence _mainDisplaySequence;
    private Sequence _notificationSequence;

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
        _isObjectiveActive = newMainObjective != null;
        if (!_hasShownFirstObjective)
        {
            _pendingFirstObjective = newMainObjective;
        }
        else
        {
            UpdateMainObjectiveDisplay(newMainObjective, true, true);
        }
    }

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

        UpdateMainObjectiveDisplay(currentMainObjective, true, true);
    }
    private void HandleLevelCompleted()
    {
        UpdateMainObjectiveDisplay(null, true, true, "All Objectives Complete!");
        _isObjectiveActive = false;
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

        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;

        Debug.Log($"<color=cyan>Coroutine STARTED.</color> Title: '{title}', ShouldFadeOut: {shouldFadeOut}");
        // 1. Immediately kill any sequence that was previously running.
        _mainDisplaySequence?.Kill();

        if (hintCanvasGroup != null) hintCanvasGroup.DOFade(0, fadeTime);
        if (UISoundPlayer.Instance != null) UISoundPlayer.Instance.PlayNewObjectiveSound();

        mainTitleText.text = title;

        // 2. Build a new animation sequence.
        _mainDisplaySequence = DOTween.Sequence();
        _mainDisplaySequence.Append(mainPanelCanvasGroup.DOFade(1, fadeTime)); // Fade in

        if (shouldFadeOut)
        {
            Debug.Log("<color=green>Sequence WILL fade out.</color>");
            _mainDisplaySequence.AppendInterval(displayTime); // Wait
            _mainDisplaySequence.Append(mainPanelCanvasGroup.DOFade(0, fadeTime)); // Fade out
            _mainDisplaySequence.AppendCallback(() =>
            {
                // This code runs after the fade-out is complete
                if (objectiveHintObject != null && _isObjectiveActive)
                {
                    objectiveHintObject.SetActive(true);
                    if (hintCanvasGroup != null) hintCanvasGroup.DOFade(1, fadeTime);
                }
            });
        }
        else
        {
            Debug.Log("<color=red>Sequence will NOT fade out.</color>");
        }
        Debug.Log("Waiting for sequence to complete...");
        // 3. Wait for the sequence to complete before finishing the coroutine.
        yield return _mainDisplaySequence.WaitForCompletion();
        Debug.Log("<color=lime>Sequence COMPLETED.</color>");
    }

    public void ShowNotification(string text)
    {
        if (_notificationCoroutine != null) StopCoroutine(_notificationCoroutine);
        _notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(text));
    }

    private IEnumerator ShowNotificationCoroutine(string text)
    {
        if (objectiveHintObject != null) objectiveHintObject.SetActive(false);
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;

        // 1. Kill any and all running animations on BOTH panels to prevent any conflict.
        _notificationSequence?.Kill();
        _mainDisplaySequence?.Kill();

        // --- Step 2: Populate the Main Objective Panel ---
        // Get the current main objective that should be displayed.
        var currentMainObjective = ObjectiveManager.Instance.CurrentObjective;
        if (currentMainObjective != null)
        {
            // Set the main title text.
            mainTitleText.text = currentMainObjective.objectiveTitle;

            // Set the main progress text, just like a normal update.
            var progressData = ObjectiveManager.Instance.GetCurrentProgressData();
            if (progressData.HasValue && progressData.Value.objectiveID == currentMainObjective.objectiveID)
            {
                mainCounterText.text = progressData.Value.counterLabel;
                mainProgressText.text = $"{progressData.Value.currentProgress} / {progressData.Value.requiredAmount}";
            }
            else
            {
                mainCounterText.text = "";
                mainProgressText.text = "";
            }
        }
        else
        {
            // If there's no main objective, the panel will correctly be blank.
            mainTitleText.text = "";
            mainCounterText.text = "";
            mainProgressText.text = "";
        }

        // --- Step 3: Populate the Notification Panel ---
        if (notificationText != null) notificationText.text = text;

        // --- Step 4: Animate Both Panels Simultaneously ---
        if (notificationCanvasGroup != null)
        {
            _notificationSequence = DOTween.Sequence();

            // Use .Join() to fade both fully-populated panels in at the exact same time.
            _notificationSequence.Append(notificationCanvasGroup.DOFade(1, fadeTime))
                                 .Join(mainPanelCanvasGroup.DOFade(1, fadeTime));

            _notificationSequence.AppendInterval(displayTime);

            // Use .Join() again to fade both panels out simultaneously.
            _notificationSequence.Append(notificationCanvasGroup.DOFade(0, fadeTime))
                                 .Join(mainPanelCanvasGroup.DOFade(0, fadeTime));

            _notificationSequence.AppendCallback(() => {
                if (objectiveHintObject != null && _isObjectiveActive)
                {
                    objectiveHintObject.SetActive(true);
                    if (hintCanvasGroup != null) hintCanvasGroup.DOFade(1, fadeTime);
                }
            });
        }

        yield return _notificationSequence.WaitForCompletion();
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
        string objectiveName = overrideText ?? (objective != null ? objective.objectiveTitle : "NULL");
        Debug.Log($"<color=yellow>UpdateMainObjectiveDisplay CALLED.</color> Objective: '{objectiveName}', ShouldFadeOut: {shouldFadeOut}");



        if (_mainDisplayCoroutine != null)
        {
            
            Debug.Log("<color=orange>Stopping existing coroutine.</color>");
            StopCoroutine(_mainDisplayCoroutine);
        }

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