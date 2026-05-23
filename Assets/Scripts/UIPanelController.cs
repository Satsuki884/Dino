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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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

        // Якщо нічого не відкрито — відкриваємо натиснуту панель.
        if (currentOpenPanel == null)
        {
            OpenPanel(targetPanel, targetPanelRect);
            return;
        }

        // Якщо натиснули кнопку тієї самої панелі — закриваємо її.
        if (currentOpenPanel == targetPanel)
        {
            CloseAllPanels();
            return;
        }

        // Якщо відкрита інша панель — тільки закриваємо поточну.
        // Нову панель гравець відкриє другим натисканням.
        CloseAllPanels();
    }

    private void OpenPanel(GameObject panel, RectTransform panelRect)
    {
        CloseAllPanels();

        panel.SetActive(true);

        currentOpenPanel = panel;
        currentOpenPanelRect = panelRect;
    }

    public void CloseAllPanels()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (foodInventoryPanel != null)
            foodInventoryPanel.SetActive(false);

        currentOpenPanel = null;
        currentOpenPanelRect = null;
    }

    private bool IsClickInsideRect(RectTransform rect, Vector2 screenPosition)
    {
        if (rect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null);
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

        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null);
    }

    private bool WasPointerPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
#else
        if (Input.GetMouseButtonDown(0))
            return true;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
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