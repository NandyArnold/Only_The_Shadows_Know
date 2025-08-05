using UnityEngine;

public class RevealableEntity : MonoBehaviour
{
    [Tooltip("The category this entity belongs to for detection skills like Balor's Vision and Scrying.")]
    public RevealableType type = RevealableType.None;
}