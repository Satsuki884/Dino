using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

    [Header("Mini Menu")]
    public GameObject miniMenuPanel;
    public Button miniMenuButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Close Settings")]
    public bool closeWhenClickOutside = true;

    private GameObject currentOpenPanel;
    private RectTransform currentOpenPanelRect;

    private bool ignoreNextOutsideClick;
    private bool foodPanelHiddenDuringDrag;
    private GameObject miniMenuInputBlocker;
    private CanvasGroup miniMenuCanvasGroup;
    private Canvas miniMenuCanvas;
    private bool originalMiniMenuCanvasOverrideSorting;
    private int originalMiniMenuCanvasSortingOrder;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (foodInventoryCanvasGroup == null && foodInventoryPanel != null)
            foodInventoryCanvasGroup = foodInventoryPanel.GetComponent<CanvasGroup>();

        FindMiniMenuReferences();
        ConfigureMiniMenu();

        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopButtonClicked);

        if (foodButton != null)
            foodButton.onClick.AddListener(OnFoodButtonClicked);

        if (shopCloseButton != null)
            shopCloseButton.onClick.AddListener(OnCloseButtonClicked);

        if (foodCloseButton != null)
            foodCloseButton.onClick.AddListener(OnCloseButtonClicked);

        CloseAllPanels();
        CloseMiniMenu();
    }

    private void Update()
    {
        if (HandleMiniMenuButtonFallback())
            return;

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
        PlayClick();
        HandlePanelButtonClick(shopPanel, shopPanelRect);
    }

    private void OnFoodButtonClicked()
    {
        PlayClick();
        HandlePanelButtonClick(foodInventoryPanel, foodInventoryPanelRect);
    }

    private void OnCloseButtonClicked()
    {
        PlayClick();
        CloseAllPanels();
    }

    private void OnMiniMenuButtonClicked()
    {
        if (IsMiniMenuOpen())
            return;

        PlayClick();
        OpenMiniMenu();
    }

    private void OnResumeButtonClicked()
    {
        PlayClick();
        CloseMiniMenu();
    }

    private void OnMainMenuButtonClicked()
    {
        PlayClick();

        if (GameManager.Instance != null)
            GameManager.Instance.SaveGame();

        SceneManager.LoadScene("Main Menu");
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

    public void OpenMiniMenu()
    {
        if (miniMenuPanel == null)
            return;

        BindMiniMenuVolumeSliders();
        CloseAllPanels();
        EnsureMiniMenuInputBlocker();

        if (miniMenuInputBlocker != null)
        {
            miniMenuInputBlocker.SetActive(true);
            miniMenuInputBlocker.transform.SetAsLastSibling();
        }

        RaiseMiniMenuCanvas();

        miniMenuPanel.SetActive(true);
        miniMenuPanel.transform.SetAsLastSibling();

        if (miniMenuCanvasGroup != null)
        {
            miniMenuCanvasGroup.alpha = 1f;
            miniMenuCanvasGroup.interactable = true;
            miniMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    public void CloseMiniMenu()
    {
        if (miniMenuPanel != null)
            miniMenuPanel.SetActive(false);

        if (miniMenuInputBlocker != null)
            miniMenuInputBlocker.SetActive(false);

        RestoreMiniMenuCanvas();

        if (miniMenuCanvasGroup != null)
        {
            miniMenuCanvasGroup.alpha = 1f;
            miniMenuCanvasGroup.interactable = true;
            miniMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    private bool HandleMiniMenuButtonFallback()
    {
        if (IsMiniMenuOpen())
            return false;

        if (miniMenuButton == null)
            return false;

        if (!WasPointerPressedThisFrame())
            return false;

        Vector2 screenPosition = GetPointerScreenPosition();

        if (!IsClickInsideButton(miniMenuButton, screenPosition))
            return false;

        OnMiniMenuButtonClicked();
        return true;
    }

    private bool IsMiniMenuOpen()
    {
        return miniMenuPanel != null && miniMenuPanel.activeSelf;
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

    private void PlayClick()
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
    }

    private void FindMiniMenuReferences()
    {
        if (miniMenuPanel == null)
            miniMenuPanel = FindSceneObject("MinMenu_panel");

        if (miniMenuButton == null)
            miniMenuButton = FindButton("MinMenu_button");

        if (resumeButton == null)
            resumeButton = FindButton("Resume_but");

        if (mainMenuButton == null)
            mainMenuButton = FindButton("Main_menu_but");

        if (musicVolumeSlider == null)
            musicVolumeSlider = FindSlider("Music_vol");

        if (sfxVolumeSlider == null)
            sfxVolumeSlider = FindSlider("SFX_vol");

        if (miniMenuPanel != null)
        {
            miniMenuCanvasGroup = miniMenuPanel.GetComponent<CanvasGroup>();
            miniMenuCanvas = miniMenuPanel.GetComponentInParent<Canvas>();

            if (miniMenuCanvas != null)
            {
                originalMiniMenuCanvasOverrideSorting = miniMenuCanvas.overrideSorting;
                originalMiniMenuCanvasSortingOrder = miniMenuCanvas.sortingOrder;
            }
        }
    }

    private void ConfigureMiniMenu()
    {
        if (miniMenuButton != null)
        {
            miniMenuButton.onClick.RemoveListener(OnMiniMenuButtonClicked);
            miniMenuButton.onClick.AddListener(OnMiniMenuButtonClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }

        if (musicVolumeSlider != null)
        {
            AddClickFeedback(musicVolumeSlider.gameObject);
        }

        if (sfxVolumeSlider != null)
        {
            AddClickFeedback(sfxVolumeSlider.gameObject);
        }

        BindMiniMenuVolumeSliders();
        EnsureMiniMenuInputBlocker();
    }

    private void BindMiniMenuVolumeSliders()
    {
        if (AudioManager.Instanse == null)
            return;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(AudioManager.Instanse.SetMusicVolume);
            musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instanse.SetMusicVolume);
            AudioManager.Instanse.SetMusicVolume(musicVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(AudioManager.Instanse.SetSFXVolume);
            sfxVolumeSlider.onValueChanged.AddListener(AudioManager.Instanse.SetSFXVolume);
            AudioManager.Instanse.SetSFXVolume(sfxVolumeSlider.value);
        }
    }

    private void EnsureMiniMenuInputBlocker()
    {
        if (miniMenuInputBlocker != null)
            return;

        if (miniMenuPanel == null)
            return;

        Transform parent = miniMenuPanel.transform.parent;

        if (parent == null)
            return;

        miniMenuInputBlocker = new GameObject("MiniMenu_InputBlocker");
        miniMenuInputBlocker.transform.SetParent(parent, false);

        RectTransform rect = miniMenuInputBlocker.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = miniMenuInputBlocker.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;

        miniMenuInputBlocker.SetActive(false);
    }

    private void RaiseMiniMenuCanvas()
    {
        if (miniMenuCanvas == null && miniMenuPanel != null)
            miniMenuCanvas = miniMenuPanel.GetComponentInParent<Canvas>();

        if (miniMenuCanvas == null)
            return;

        miniMenuCanvas.overrideSorting = true;
        miniMenuCanvas.sortingOrder = 1000;
    }

    private void RestoreMiniMenuCanvas()
    {
        if (miniMenuCanvas == null)
            return;

        miniMenuCanvas.overrideSorting = originalMiniMenuCanvasOverrideSorting;
        miniMenuCanvas.sortingOrder = originalMiniMenuCanvasSortingOrder;
    }

    private Button FindButton(string objectName)
    {
        GameObject found = FindSceneObject(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private Slider FindSlider(string objectName)
    {
        GameObject found = FindSceneObject(objectName);
        return found != null ? found.GetComponent<Slider>() : null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject found = GameObject.Find(objectName);

        if (found != null)
            return found;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null)
                continue;

            if (transform.name != objectName)
                continue;

            if (!transform.gameObject.scene.IsValid())
                continue;

            return transform.gameObject;
        }

        return null;
    }

    private void AddClickFeedback(GameObject target)
    {
        if (target == null)
            return;

        if (target.GetComponent<UIAudioClickFeedback>() == null)
            target.AddComponent<UIAudioClickFeedback>();
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

public class UIAudioClickFeedback : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
    }
}
