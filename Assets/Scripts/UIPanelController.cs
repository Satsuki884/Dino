using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIPanelController : MonoBehaviour
{
    public static UIPanelController Instance;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject foodInventoryPanel;

    [Header("Panel Rects")]
    public RectTransform shopPanelRect;
    public RectTransform foodInventoryPanelRect;

    [Header("Canvas Groups")]
    public CanvasGroup foodInventoryCanvasGroup;

    [Header("Open Buttons")]
    public Button shopButton;
    public Button foodButton;

    [Header("Close Buttons")]
    public Button shopCloseButton;
    public Button foodCloseButton;

    [Header("Close Settings")]
    public bool closeWhenClickOutside = true;

    private GameObject currentOpenPanel;
    private RectTransform currentOpenPanelRect;

    private bool ignoreNextOutsideClick;
    private bool foodPanelHiddenDuringDrag;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (foodInventoryCanvasGroup == null && foodInventoryPanel != null)
            foodInventoryCanvasGroup = foodInventoryPanel.GetComponent<CanvasGroup>();

        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopButtonClicked);

        if (foodButton != null)
            foodButton.onClick.AddListener(OnFoodButtonClicked);

        if (shopCloseButton != null)
            shopCloseButton.onClick.AddListener(CloseAllPanels);

        if (foodCloseButton != null)
            foodCloseButton.onClick.AddListener(CloseAllPanels);

        CloseAllPanels();
    }

    private void Update()
    {
        if (DraggableFoodUI.IsDraggingFood)
            return;

        if (!closeWhenClickOutside)
            return;

        if (currentOpenPanel == null)
            return;

        if (!WasPointerPressedThisFrame())
            return;

        if (ignoreNextOutsideClick)
        {
            ignoreNextOutsideClick = false;
            return;
        }

        Vector2 screenPosition = GetPointerScreenPosition();

        if (IsClickInsideRect(currentOpenPanelRect, screenPosition))
            return;

        if (IsClickOnButton(screenPosition))
            return;

        CloseAllPanels();
    }

    private void OnShopButtonClicked()
    {
        HandlePanelButtonClick(shopPanel, shopPanelRect);
    }

    private void OnFoodButtonClicked()
    {
        HandlePanelButtonClick(foodInventoryPanel, foodInventoryPanelRect);
    }

    private void HandlePanelButtonClick(GameObject targetPanel, RectTransform targetPanelRect)
    {
        if (targetPanel == null)
            return;

        ignoreNextOutsideClick = true;

        if (currentOpenPanel == null)
        {
            OpenPanel(targetPanel, targetPanelRect);
            return;
        }

        if (currentOpenPanel == targetPanel)
        {
            CloseAllPanels();
            return;
        }

        CloseAllPanels();
    }

    private void OpenPanel(GameObject panel, RectTransform panelRect)
    {
        CloseAllPanels();

        panel.SetActive(true);

        if (panel == foodInventoryPanel)
            ShowFoodPanelVisuals();

        currentOpenPanel = panel;
        currentOpenPanelRect = panelRect;
    }

    public void CloseAllPanels()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (foodInventoryPanel != null)
            foodInventoryPanel.SetActive(false);

        ShowFoodPanelVisuals();

        currentOpenPanel = null;
        currentOpenPanelRect = null;
        foodPanelHiddenDuringDrag = false;
    }

    public void CloseFoodPanelOnly()
    {
        if (foodInventoryPanel != null)
            foodInventoryPanel.SetActive(false);

        ShowFoodPanelVisuals();

        if (currentOpenPanel == foodInventoryPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }

        foodPanelHiddenDuringDrag = false;
    }

    public void CloseShopPanelOnly()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (currentOpenPanel == shopPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }
    }

    public void HideFoodPanelDuringDrag()
    {
        if (foodInventoryPanel == null)
            return;

        if (!foodInventoryPanel.activeSelf)
            return;

        if (foodInventoryCanvasGroup == null)
            foodInventoryCanvasGroup = foodInventoryPanel.GetComponent<CanvasGroup>();

        if (foodInventoryCanvasGroup == null)
        {
            Debug.LogWarning("FoodInventoryPanel has no CanvasGroup.");
            return;
        }

        foodInventoryCanvasGroup.alpha = 0f;
        foodInventoryCanvasGroup.interactable = false;
        foodInventoryCanvasGroup.blocksRaycasts = false;

        foodPanelHiddenDuringDrag = true;
    }

    public void FinishFoodPanelDragClose()
    {
        if (!foodPanelHiddenDuringDrag)
            return;

        if (foodInventoryPanel != null)
            foodInventoryPanel.SetActive(false);

        ShowFoodPanelVisuals();

        if (currentOpenPanel == foodInventoryPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }

        foodPanelHiddenDuringDrag = false;
    }

    private void ShowFoodPanelVisuals()
    {
        if (foodInventoryCanvasGroup == null && foodInventoryPanel != null)
            foodInventoryCanvasGroup = foodInventoryPanel.GetComponent<CanvasGroup>();

        if (foodInventoryCanvasGroup == null)
            return;

        foodInventoryCanvasGroup.alpha = 1f;
        foodInventoryCanvasGroup.interactable = true;
        foodInventoryCanvasGroup.blocksRaycasts = true;
    }

    public RectTransform GetFoodPanelRect()
    {
        return foodInventoryPanelRect;
    }

    public RectTransform GetShopPanelRect()
    {
        return shopPanelRect;
    }

    public bool IsAnyPanelOpen()
    {
        return currentOpenPanel != null;
    }

    public bool IsFoodPanelOpen()
    {
        return currentOpenPanel == foodInventoryPanel;
    }

    public bool IsShopPanelOpen()
    {
        return currentOpenPanel == shopPanel;
    }

    private bool IsClickInsideRect(RectTransform rect, Vector2 screenPosition)
    {
        if (rect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            screenPosition,
            null
        );
    }

    private bool IsClickOnButton(Vector2 screenPosition)
    {
        if (IsClickInsideButton(shopButton, screenPosition))
            return true;

        if (IsClickInsideButton(foodButton, screenPosition))
            return true;

        if (IsClickInsideButton(shopCloseButton, screenPosition))
            return true;

        if (IsClickInsideButton(foodCloseButton, screenPosition))
            return true;

        return false;
    }

    private bool IsClickInsideButton(Button button, Vector2 screenPosition)
    {
        if (button == null)
            return false;

        RectTransform rect = button.GetComponent<RectTransform>();

        if (rect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            screenPosition,
            null
        );
    }

    private bool WasPointerPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
#else
        if (Input.GetMouseButtonDown(0))
            return true;

        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
            return true;

        return false;
#endif
    }

    private Vector2 GetPointerScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        return Vector2.zero;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;

        return Input.mousePosition;
#endif
    }
}