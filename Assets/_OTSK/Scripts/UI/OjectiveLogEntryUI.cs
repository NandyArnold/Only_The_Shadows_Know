// QuestLogEntryUI.cs
using TMPro;
using UnityEngine;

public class ObjectiveLogEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    // You could add an Image for a quest type icon here too

    public void Populate(ObjectiveSO objective)
    {
        if (objective == null) return;

        titleText.text = objective.objectiveTitle;
        descriptionText.text = objective.objectiveDescription;
    }
}
