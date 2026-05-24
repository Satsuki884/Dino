using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIPanelController : MonoBehaviour
{
    public static UIPanelController Instance;

    [Header("Dino Shop")]
    [FormerlySerializedAs("shopPanel")] public GameObject dinoShopPanel;
    [FormerlySerializedAs("shopPanelRect")] public RectTransform dinoShopPanelRect;
    [FormerlySerializedAs("shopButton")] public Button dinoShopButton;
    [FormerlySerializedAs("shopCloseButton")] public Button dinoShopCloseButton;

    [Header("Food Shop")]
    public GameObject foodShopPanel;
    public RectTransform foodShopPanelRect;
    [FormerlySerializedAs("foodButton")] public Button foodShopButton;
    [FormerlySerializedAs("foodCloseButton")] public Button foodShopCloseButton;

    [Header("Food Inventory")]
    public GameObject foodInventoryPanel;
    public RectTransform foodInventoryPanelRect;
    public CanvasGroup foodInventoryCanvasGroup;

    [Header("Close Settings")]
    public bool closeWhenClickOutside = true;

    [Header("Mini Menu")]
    public GameObject miniMenuPanel;
    public Button miniMenuButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private GameObject currentOpenPanel;
    private RectTransform currentOpenPanelRect;

    private bool foodPanelHiddenDuringDrag;
    private GameObject miniMenuInputBlocker;
    private CanvasGroup miniMenuCanvasGroup;
    private Canvas miniMenuCanvas;
    private bool originalMiniMenuCanvasOverrideSorting;
    private int originalMiniMenuCanvasSortingOrder;

    private bool ignoreNextOutsideClick;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindMiniMenuReferences();
        ConfigureMiniMenu();
        CloseMiniMenu();

        if (foodInventoryCanvasGroup == null && foodInventoryPanel != null)
            foodInventoryCanvasGroup = foodInventoryPanel.GetComponent<CanvasGroup>();

        if (dinoShopButton != null)
            dinoShopButton.onClick.AddListener(OnDinoShopButtonClicked);

        if (foodShopButton != null)
            foodShopButton.onClick.AddListener(OnFoodShopButtonClicked);

        if (dinoShopCloseButton != null)
            dinoShopCloseButton.onClick.AddListener(OnCloseButtonClicked);

        if (foodShopCloseButton != null)
            foodShopCloseButton.onClick.AddListener(OnCloseButtonClicked);

        if (foodShopPanel == null)
            Debug.LogWarning("FoodShopPanel is not assigned in UIPanelController. Assign the food shop panel separately from FoodInventoryPanel.");

        if (foodInventoryPanel == null)
            Debug.LogWarning("FoodInventoryPanel is not assigned in UIPanelController. Food inventory should stay visible.");

        CloseAllPanels();
        ShowFoodInventoryPanel();
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

    private void OnDinoShopButtonClicked()
    {
        PlayClick();
        HandlePanelButtonClick(dinoShopPanel, dinoShopPanelRect);
    }

    private void OnFoodShopButtonClicked()
    {
        PlayClick();
        HandlePanelButtonClick(foodShopPanel, foodShopPanelRect);
    }

    private void OnCloseButtonClicked()
    {
        PlayClick();
        CloseAllPanels();
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
        if (dinoShopPanel != null)
            dinoShopPanel.SetActive(false);

        if (foodShopPanel != null)
            foodShopPanel.SetActive(false);

        ShowFoodPanelVisuals();

        currentOpenPanel = null;
        currentOpenPanelRect = null;
    }

    public void CloseFoodPanelOnly()
    {
        ShowFoodInventoryPanel();
    }

    public void CloseShopPanelOnly()
    {
        if (dinoShopPanel != null)
            dinoShopPanel.SetActive(false);

        if (currentOpenPanel == dinoShopPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }
    }

    public void CloseFoodShopPanelOnly()
    {
        if (foodShopPanel != null)
            foodShopPanel.SetActive(false);

        if (currentOpenPanel == foodShopPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }
    }

    public void HideFoodPanelDuringDrag()
    {
        ShowFoodInventoryPanel();
    }

    public void FinishFoodPanelDragClose()
    {
        ShowFoodInventoryPanel();
    }

    private void ShowFoodInventoryPanel()
    {
        if (foodInventoryPanel != null)
            foodInventoryPanel.SetActive(true);

        ShowFoodPanelVisuals();

        if (currentOpenPanel == foodInventoryPanel)
        {
            currentOpenPanel = null;
            currentOpenPanelRect = null;
        }
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
        return dinoShopPanelRect;
    }

    public bool IsAnyPanelOpen()
    {
        return currentOpenPanel != null;
    }

    public bool IsFoodPanelOpen()
    {
        return foodInventoryPanel != null && foodInventoryPanel.activeSelf;
    }

    public bool IsShopPanelOpen()
    {
        return currentOpenPanel == dinoShopPanel || currentOpenPanel == foodShopPanel;
    }

    public bool IsDinoShopPanelOpen()
    {
        return currentOpenPanel == dinoShopPanel;
    }

    public bool IsFoodShopPanelOpen()
    {
        return currentOpenPanel == foodShopPanel;
    }

    private void PlayClick()
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
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
        if (IsClickOnAnyUIButton(screenPosition))
            return true;

        if (IsClickInsideButton(dinoShopButton, screenPosition))
            return true;

        if (IsClickInsideButton(foodShopButton, screenPosition))
            return true;

        if (IsClickInsideButton(dinoShopCloseButton, screenPosition))
            return true;

        if (IsClickInsideButton(foodShopCloseButton, screenPosition))
            return true;

        return false;
    }

    private bool IsClickOnAnyUIButton(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, uiRaycastResults);

        foreach (RaycastResult result in uiRaycastResults)
        {
            if (result.gameObject == null)
                continue;

            if (result.gameObject.GetComponentInParent<Button>() != null)
                return true;
        }

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

    private void OnMiniMenuButtonClicked()
    {
        if (IsMiniMenuOpen())
            return;

        PlayClick();
        OpenMiniMenu();
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

    private bool IsMiniMenuOpen()
    {
        return miniMenuPanel != null && miniMenuPanel.activeSelf;
    }

    private void OnResumeButtonClicked()
    {
        PlayClick();
        CloseMiniMenu();
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

    private void OnMainMenuButtonClicked()
    {
        PlayClick();

        if (GameManager.Instance != null)
            GameManager.Instance.SaveGame();

        SceneManager.LoadScene("Main Menu");
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

}


public class UIAudioClickFeedback : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
    }
}
