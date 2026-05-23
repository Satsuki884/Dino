using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableFoodUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IInitializePotentialDragHandler
{
    public static bool IsDraggingFood { get; private set; }

    [Header("Drag")]
    public Image dragIconPrefab;

    [Header("Settings")]
    public bool disableScrollWhileDragging = true;
    public bool closeInventoryWhenDraggedOutside = true;

    private FoodConfig foodConfig;
    private Image currentDragIcon;
    private Canvas rootCanvas;
    private ScrollRect parentScrollRect;

    private bool isAvailable;
    private bool isDragging;
    private bool inventoryWasClosed;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void Init(FoodConfig config)
    {
        foodConfig = config;
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;

        Image image = GetComponent<Image>();

        if (image != null)
        {
            image.raycastTarget = true;
            image.color = isAvailable ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;

        isDragging = true;
        IsDraggingFood = true;
        inventoryWasClosed = false;

        if (disableScrollWhileDragging && parentScrollRect != null)
            parentScrollRect.enabled = false;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        currentDragIcon = Instantiate(dragIconPrefab, rootCanvas.transform);

        currentDragIcon.sprite = foodConfig.icon;
        currentDragIcon.preserveAspect = true;
        currentDragIcon.raycastTarget = false;

        RectTransform dragRect = currentDragIcon.GetComponent<RectTransform>();
        dragRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (currentDragIcon != null)
            currentDragIcon.transform.position = eventData.position;

        if (closeInventoryWhenDraggedOutside)
            TryCloseInventoryIfDraggedOutside(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (currentDragIcon != null)
            Destroy(currentDragIcon.gameObject);

        bool wasFed = TryFeedDino(eventData.position);

        if (wasFed)
        {
            FoodInventory.Instance.UseFood(foodConfig);
        }
        else
        {
            FoodInventory.Instance.RefreshInventoryUI();
        }

        if (disableScrollWhileDragging && parentScrollRect != null)
            parentScrollRect.enabled = true;

        isDragging = false;
        IsDraggingFood = false;
        inventoryWasClosed = false;
    }

    private bool CanDrag()
    {
        if (!isAvailable)
            return false;

        if (foodConfig == null)
            return false;

        if (FoodInventory.Instance == null)
            return false;

        if (!FoodInventory.Instance.HasFood(foodConfig))
            return false;

        if (dragIconPrefab == null)
        {
            Debug.LogWarning("Drag Icon Prefab is not assigned.");
            return false;
        }

        return true;
    }

    private void TryCloseInventoryIfDraggedOutside(Vector2 screenPosition)
    {
        if (inventoryWasClosed)
            return;

        if (UIPanelController.Instance == null)
            return;

        RectTransform foodPanelRect = UIPanelController.Instance.GetFoodPanelRect();

        if (foodPanelRect == null)
            return;

        bool insideInventory = RectTransformUtility.RectangleContainsScreenPoint(
            foodPanelRect,
            screenPosition,
            null
        );

        if (insideInventory)
            return;

        UIPanelController.Instance.CloseFoodPanelOnly();
        inventoryWasClosed = true;
    }

    private bool TryFeedDino(Vector2 screenPosition)
    {
        Camera camera = Camera.main;

        if (camera == null)
            return false;

        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in hits)
        {
            Dino dino = hit.GetComponentInParent<Dino>();

            if (dino == null)
                continue;

            return dino.Feed(foodConfig);
        }

        return false;
    }
}