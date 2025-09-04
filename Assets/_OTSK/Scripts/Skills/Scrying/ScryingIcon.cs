// ScryingIcon.cs
using UnityEngine;
using UnityEngine.UI;

public class ScryingIcon : MonoBehaviour
{
    public Image iconImage;

    public void FaceCamera(Transform cameraTransform)
    {
        transform.rotation = cameraTransform.rotation;
    }
}