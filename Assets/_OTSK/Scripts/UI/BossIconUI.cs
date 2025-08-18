// Create this new script, BossIconUI.cs
using UnityEngine;

public class BossIconUI : MonoBehaviour
{
    [Header("Scaling Settings")]
    [Tooltip("The scale of the icon at a distance of 1 meter.")]
    [SerializeField] private float scaleAtOneMeter = 1f;
    [Tooltip("The minimum scale the icon can shrink to.")]
    [SerializeField] private float minScale = 0.5f;
    [Tooltip("The maximum scale the icon can grow to.")]
    [SerializeField] private float maxScale = 2f;

    private RectTransform _rectTransform;
    public Transform targetToFollow;
    private Camera _mainCamera;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetToFollow == null || _mainCamera == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- THIS IS THE NEW LOGIC ---

        // 1. Get the screen position. The 'z' value is the distance from the camera.
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(targetToFollow.position);

        // 2. Calculate the desired scale based on distance.
        //    We divide the base scale by the distance to make it smaller as it gets farther away.
        float distance = screenPos.z;
        float desiredScale = scaleAtOneMeter / distance;

        // 3. Clamp the scale between our min and max values.
        float finalScale = Mathf.Clamp(desiredScale, minScale, maxScale);

        // 4. Apply the position and the new scale.
        _rectTransform.position = screenPos;
        _rectTransform.localScale = new Vector3(finalScale, finalScale, 1f);

        // 5. Only show the icon if the target is in front of the camera.
        bool isVisible = (screenPos.z > 0);
        _rectTransform.gameObject.SetActive(isVisible);
    }
}