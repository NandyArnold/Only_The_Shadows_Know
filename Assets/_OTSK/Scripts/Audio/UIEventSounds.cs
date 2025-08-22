// UIEventSounds.cs
using UnityEngine;
using UnityEngine.EventSystems; // Required for the event interfaces

// This script will automatically listen for mouse hover and click events.
public class UIEventSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public enum ClickSoundType { Confirm, Back }

    [Tooltip("Which type of click sound should this button play?")]
    [SerializeField] private ClickSoundType clickType = ClickSoundType.Confirm;

    [Tooltip("Should this button play a sound on hover?")]
    [SerializeField] private bool playHoverSound = true;

    // This method is called automatically when the mouse hovers over this UI element.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && UISoundPlayer.Instance != null)
        {
            UISoundPlayer.Instance.PlayHoverSound();
        }
    }

    // This method is called automatically when this UI element is clicked.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UISoundPlayer.Instance != null)
        {
            if (clickType == ClickSoundType.Confirm)
            {
                UISoundPlayer.Instance.PlayClickConfirmSound();
            }
            else
            {
                UISoundPlayer.Instance.PlayClickBackSound();
            }
        }
    }
}