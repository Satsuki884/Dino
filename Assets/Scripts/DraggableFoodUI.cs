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

    [Header("Dino Detection")]
    public float detectionRadius = 0.7f;

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
            Debug.LogWarning(name + ": DragArea has no Image. Drag will not work.");

        if (image != null && !image.raycastTarget)
            Debug.LogWarning(name + ": DragArea Image Raycast Target is disabled. Drag will not work.");
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

            // DragArea має ловити drag, але НЕ має бути видимим.
            image.color = new Color(1f, 1f, 1f, 0f);
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

        TryUseFoodOnDino(eventData.position);

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
            Debug.LogWarning(name + ": food is not available. Maybe amount = 0.");
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
            Debug.LogWarning(name + ": no food in inventory: " + foodConfig.foodName);
            return false;
        }

        if (dragIconPrefab == null)
        {
            Debug.LogWarning(name + ": Drag Icon Prefab is not assigned.");
            return false;
        }

        if (rootCanvas == null)
        {
            Debug.LogWarning(name + ": rootCanvas == null.");
            return false;
        }

        return true;
    }

    private void TryUseFoodOnDino(Vector2 screenPosition)
    {
        Dino targetDino = FindDinoUnderPointer(screenPosition);

        if (targetDino == null)
        {
            if (showDebugLogs)
                Debug.Log("Food was not dropped on dino. Food is not used.");

            FoodInventory.Instance.RefreshInventoryUI();
            return;
        }

        bool foodWasUsed = FoodInventory.Instance.UseFood(foodConfig);

        if (!foodWasUsed)
        {
            if (showDebugLogs)
                Debug.LogWarning("Could not use food from inventory: " + foodConfig.foodName);

            FoodInventory.Instance.RefreshInventoryUI();
            return;
        }

        targetDino.Feed(foodConfig);

        if (showDebugLogs)
        {
            int amountLeft = FoodInventory.Instance.GetFoodAmount(foodConfig);
            Debug.Log("Fed dino with " + foodConfig.foodName + ". Food left: " + amountLeft);
        }
    }

    private Dino FindDinoUnderPointer(Vector2 screenPosition)
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            Dino dino = hit.GetComponentInParent<Dino>();

            if (dino == null)
                continue;

            return dino;
        }

        return null;
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
}