using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public class RaidController : MonoBehaviour
{
    [Header("Camera")]
    public Camera raidCamera;

    [Header("Spawn")]
    public Transform dinoViewParent;
    public RaidDinoView dinoViewPrefab;
    public BoxCollider2D meadowBounds;

    [Header("Buttons")]
    public Button backButton;

    [Header("Confirm Popup")]
    public GameObject confirmPopup;
    public TMP_Text confirmText;
    public Button yesButton;
    public Button noButton;

    private readonly Dictionary<RaidEntry, RaidDinoView> spawnedViews = new Dictionary<RaidEntry, RaidDinoView>();

    private RaidEntry pendingReturnEntry;

    private void OnEnable()
    {
        if (RaidManager.Instance != null)
            RaidManager.Instance.OnRaidsChanged += RebuildViews;
    }

    private void OnDisable()
    {
        if (RaidManager.Instance != null)
            RaidManager.Instance.OnRaidsChanged -= RebuildViews;
    }

    private void Start()
    {
        if (raidCamera == null)
            raidCamera = Camera.main;

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainScene);

        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmReturnDino);

        if (noButton != null)
            noButton.onClick.AddListener(HideConfirmPopup);

        HideConfirmPopup();
        RebuildViews();
    }

    private void Update()
    {
        foreach (RaidDinoView view in spawnedViews.Values)
        {
            if (view != null)
                view.Refresh();
        }

        HandleRaidDinoClick();
    }

    private void RebuildViews()
    {
        ClearViews();

        if (RaidManager.Instance == null)
            return;

        foreach (RaidEntry entry in RaidManager.Instance.ActiveRaids)
        {
            CreateView(entry);
        }
    }

    private void CreateView(RaidEntry entry)
    {
        if (entry == null)
            return;

        if (dinoViewPrefab == null)
            return;

        Vector3 spawnPosition = GetRandomPointInMeadow();

        Transform parent = dinoViewParent != null ? dinoViewParent : transform;

        RaidDinoView view = Instantiate(dinoViewPrefab, spawnPosition, Quaternion.identity, parent);

        Bounds bounds = meadowBounds != null
            ? meadowBounds.bounds
            : new Bounds(Vector3.zero, new Vector3(10f, 6f, 0f));

        view.Init(entry, this, bounds);

        spawnedViews.Add(entry, view);
    }

    private void ClearViews()
    {
        foreach (RaidDinoView view in spawnedViews.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        spawnedViews.Clear();
    }

    private Vector3 GetRandomPointInMeadow()
    {
        if (meadowBounds == null)
            return Vector3.zero;

        Bounds bounds = meadowBounds.bounds;

        float x = Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f);
        float y = Random.Range(bounds.min.y + 0.5f, bounds.max.y - 0.5f);

        return new Vector3(x, y, 0f);
    }

    public void AskReturnDino(RaidEntry entry)
    {
        if (entry == null)
            return;

        pendingReturnEntry = entry;

        if (confirmPopup != null)
            confirmPopup.SetActive(true);

        if (confirmText != null)
            confirmText.text = "Are you sure you want to return the dinosaur?\nNo reward will be received for this raid.";
    }

    private void ConfirmReturnDino()
    {
        if (pendingReturnEntry != null && RaidManager.Instance != null)
        {
            RaidManager.Instance.CancelRaid(pendingReturnEntry);
        }

        pendingReturnEntry = null;
        HideConfirmPopup();
    }

    private void HideConfirmPopup()
    {
        pendingReturnEntry = null;

        if (confirmPopup != null)
            confirmPopup.SetActive(false);
    }

    private void BackToMainScene()
    {
        if (RaidSceneLoader.Instance != null)
            RaidSceneLoader.Instance.CloseRaidScene();
    }

    private void HandleRaidDinoClick()
    {
        if (!WasPointerPressedThisFrame())
            return;

        if (IsPointerOverUI())
            return;

        if (raidCamera == null)
            return;

        Vector2 screenPosition = GetPointerScreenPosition();

        Vector3 worldPosition = raidCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        if (hits == null || hits.Length == 0)
            return;

        foreach (Collider2D hit in hits)
        {
            RaidDinoView raidDinoView = hit.GetComponentInParent<RaidDinoView>();

            if (raidDinoView == null)
                continue;

            raidDinoView.Click();
            return;
        }
    }

    private bool WasPointerPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null)
        {
            TouchControl touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
                return true;
        }

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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}