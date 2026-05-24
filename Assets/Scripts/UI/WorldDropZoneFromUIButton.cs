using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WorldDropZoneFromUIButton : MonoBehaviour
{
    [SerializeField] private RectTransform targetUI;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float worldZ = 0f;
    [SerializeField] private Vector2 padding = Vector2.zero;

    private BoxCollider2D boxCollider;
    private readonly Vector3[] corners = new Vector3[4];

    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void Start()
    {
        Apply();
    }

    private void LateUpdate()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
            Apply();
    }

    public void Apply()
    {
        if (targetUI == null || worldCamera == null)
            return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        targetUI.GetWorldCorners(corners);

        Vector3 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector3 screenTopRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        float distanceFromCamera = worldZ - worldCamera.transform.position.z;

        screenBottomLeft.z = distanceFromCamera;
        screenTopRight.z = distanceFromCamera;

        Vector3 worldBottomLeft = worldCamera.ScreenToWorldPoint(screenBottomLeft);
        Vector3 worldTopRight = worldCamera.ScreenToWorldPoint(screenTopRight);

        Vector3 center = (worldBottomLeft + worldTopRight) * 0.5f;
        Vector2 size = new Vector2(
            Mathf.Abs(worldTopRight.x - worldBottomLeft.x) + padding.x,
            Mathf.Abs(worldTopRight.y - worldBottomLeft.y) + padding.y
        );

        transform.position = new Vector3(center.x, center.y, worldZ);
        transform.localScale = Vector3.one;

        boxCollider.offset = Vector2.zero;
        boxCollider.size = size;
    }
}