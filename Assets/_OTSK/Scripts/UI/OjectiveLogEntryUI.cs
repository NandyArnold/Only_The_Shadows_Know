// QuestLogEntryUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveLogEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconCompleted;
    [SerializeField] private Image iconInProgress;
    // You could add an Image for a quest type icon here too

    public void Populate(ObjectiveInstance objectiveInstance)
    {
        if (objectiveInstance == null) return;

        var objectiveSO = objectiveInstance.SourceSO;
        titleText.text = objectiveSO.objectiveTitle;
        descriptionText.text = objectiveSO.objectiveDescription;

        // Set the correct icon based on the objective's current state
        if (objectiveInstance.State == ObjectiveState.Completed)
        {
            iconCompleted.gameObject.SetActive(true);
            iconInProgress.gameObject.SetActive(false);
        }
        else // It's Active
        {
            iconCompleted.gameObject.SetActive(false);
            iconInProgress.gameObject.SetActive(true);
        }
    }
}
