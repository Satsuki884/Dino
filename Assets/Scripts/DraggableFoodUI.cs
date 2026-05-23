using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableFoodUI : MonoBehaviour,
    IPointerDownHandler,
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
    public bool showDebugLogs = true;

    private FoodConfig foodConfig;
    private Image currentDragIcon;
    private Canvas rootCanvas;
    private ScrollRect parentScrollRect;

    private bool isAvailable;
    private bool isDragging;
    private bool inventoryWasHidden;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        parentScrollRect = GetComponentInParent<ScrollRect>();

        Image image = GetComponent<Image>();

        if (image == null)
            Debug.LogWarning(name + ": на DragArea немає Image. Drag не буде працювати.");

        if (image != null && !image.raycastTarget)
            Debug.LogWarning(name + ": Image Raycast Target вимкнений. Drag не буде працювати.");
    }

    public void Init(FoodConfig config)
    {
        foodConfig = config;

        if (showDebugLogs)
            Debug.Log(name + ": Init food = " + (foodConfig != null ? foodConfig.foodName : "NULL"));
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

        if (showDebugLogs)
            Debug.Log(name + ": SetAvailable = " + isAvailable);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log(name + ": POINTER DOWN on food drag area");
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;

        if (showDebugLogs)
            Debug.Log(name + ": Initialize potential drag");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log(name + ": BEGIN DRAG");

        if (!CanDrag())
        {
            if (showDebugLogs)
                Debug.LogWarning(name + ": CanDrag returned FALSE");

            return;
        }

        isDragging = true;
        IsDraggingFood = true;
        inventoryWasHidden = false;

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
            TryHideInventoryIfDraggedOutside(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (showDebugLogs)
            Debug.Log(name + ": END DRAG");

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

        if (UIPanelController.Instance != null)
            UIPanelController.Instance.FinishFoodPanelDragClose();

        isDragging = false;
        IsDraggingFood = false;
        inventoryWasHidden = false;
    }

    private bool CanDrag()
    {
        if (!isAvailable)
        {
            Debug.LogWarning(name + ": їжа недоступна. Можливо amount = 0.");
            return false;
        }

        if (foodConfig == null)
        {
            Debug.LogWarning(name + ": foodConfig == null.");
            return false;
        }

        if (FoodInventory.Instance == null)
        {
            Debug.LogWarning(name + ": FoodInventory.Instance == null.");
            return false;
        }

        if (!FoodInventory.Instance.HasFood(foodConfig))
        {
            Debug.LogWarning(name + ": у інвентарі немає цієї їжі: " + foodConfig.foodName);
            return false;
        }

        if (dragIconPrefab == null)
        {
            Debug.LogWarning(name + ": Drag Icon Prefab не підставлений.");
            return false;
        }

        if (rootCanvas == null)
        {
            Debug.LogWarning(name + ": rootCanvas == null.");
            return false;
        }

        return true;
    }

    private void TryHideInventoryIfDraggedOutside(Vector2 screenPosition)
    {
        if (inventoryWasHidden)
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

        UIPanelController.Instance.HideFoodPanelDuringDrag();
        inventoryWasHidden = true;
    }

    private bool TryFeedDino(Vector2 screenPosition)
    {
        Camera camera = Camera.main;

        if (camera == null)
            return false;

        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        float detectionRadius = 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, detectionRadius);

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