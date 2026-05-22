using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableFoodUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag")]
    public Image dragIconPrefab;
    public Canvas rootCanvas;

    private FoodConfig foodConfig;
    private Image currentDragIcon;

    private bool isAvailable = true;
    private bool isDraggingFood = false;

    public void Init(FoodConfig config)
    {
        foodConfig = config;
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;

        Image image = GetComponent<Image>();

        if (image != null)
            image.color = isAvailable ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanStartDrag())
            return;

        isDraggingFood = true;

        currentDragIcon = Instantiate(dragIconPrefab, rootCanvas.transform);
        currentDragIcon.sprite = foodConfig.icon;
        currentDragIcon.raycastTarget = false;
        currentDragIcon.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingFood)
            return;

        if (currentDragIcon == null)
            return;

        currentDragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggingFood)
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
            // Їжа НЕ витрачається.
            // Іконка просто пропадає, а кількість в інвентарі залишається.
            FoodInventory.Instance.RefreshInventoryUI();
        }

        isDraggingFood = false;
    }

    private bool CanStartDrag()
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
            return false;

        if (rootCanvas == null)
            return false;

        return true;
    }

    private bool TryFeedDino(Vector2 screenPosition)
    {
        if (foodConfig == null)
            return false;

        Camera camera = Camera.main;

        if (camera == null)
            return false;

        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return false;

        Dino dino = hit.GetComponent<Dino>();

        if (dino == null)
            return false;

        bool fed = dino.Feed(foodConfig);

        return fed;
    }
}