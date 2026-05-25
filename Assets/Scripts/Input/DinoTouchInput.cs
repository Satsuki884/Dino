using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public class DinoTouchInput : MonoBehaviour
{
    [Header("UI Blocking")]
    public bool blockInputOverUI = true;

    [Header("Raid")]
    public RaidDropZone raidDropZone;

    private Camera mainCamera;

    private Dino selectedDino;
    private Vector3 dragOffset;
    private Vector3 selectedDinoStartPosition;

    private bool isDragging;
    private Vector2 pointerStartPosition;

    private const float dragDistance = 5f;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandlePointerInput();
    }

    private void HandlePointerInput()
    {
        bool pressedThisFrame = false;
        bool isPressed = false;
        bool releasedThisFrame = false;
        Vector2 screenPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            isPressed = Mouse.current.leftButton.isPressed;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            screenPosition = Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null)
        {
            TouchControl touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame ||
                touch.press.isPressed ||
                touch.press.wasReleasedThisFrame)
            {
                pressedThisFrame = touch.press.wasPressedThisFrame;
                isPressed = touch.press.isPressed;
                releasedThisFrame = touch.press.wasReleasedThisFrame;
                screenPosition = touch.position.ReadValue();
            }
        }
#else
        pressedThisFrame = Input.GetMouseButtonDown(0);
        isPressed = Input.GetMouseButton(0);
        releasedThisFrame = Input.GetMouseButtonUp(0);
        screenPosition = Input.mousePosition;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            pressedThisFrame = touch.phase == TouchPhase.Began;
            isPressed = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            releasedThisFrame = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            screenPosition = touch.position;
        }
#endif

        if (pressedThisFrame)
        {
            pointerStartPosition = screenPosition;

            if (TrySelectDino(screenPosition))
                return;

            if (blockInputOverUI && IsPointerOverUI())
                return;
        }

        if (isPressed && selectedDino != null)
        {
            float distance = Vector2.Distance(pointerStartPosition, screenPosition);

            if (distance > dragDistance)
                isDragging = true;

            if (isDragging)
                DragSelected(screenPosition);
        }

        if (releasedThisFrame)
        {
            ReleaseSelected();
        }
    }

    private bool TrySelectDino(Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return false;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        if (hits == null || hits.Length == 0)
            return false;

        foreach (Collider2D hit in hits)
        {
            Dino dino = hit.GetComponentInParent<Dino>();

            if (dino == null)
                continue;

            if (dino.IsInRaid())
                continue;

            selectedDino = dino;
            selectedDinoStartPosition = selectedDino.transform.position;

            dragOffset = selectedDino.transform.position - worldPosition;
            isDragging = false;

            selectedDino.SetDragging(true);

            return true;
        }

        return false;
    }

    private void DragSelected(Vector2 screenPosition)
    {
        if (selectedDino == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Vector3 targetPosition = worldPosition + dragOffset;
        targetPosition.z = 0f;

        targetPosition = ClampPositionToField(targetPosition);

        selectedDino.transform.position = targetPosition;
    }

    private void ReleaseSelected()
    {
        if (selectedDino == null)
            return;

        selectedDino.SetDragging(false);

        if (TrySendSelectedDinoToRaid())
        {
            selectedDino = null;
            isDragging = false;
            return;
        }

        if (isDragging)
            TryMergeSelectedDino();

        selectedDino = null;
        isDragging = false;
    }

    private bool TrySendSelectedDinoToRaid()
    {
        if (!isDragging)
            return false;

        if (selectedDino == null)
            return false;

        if (selectedDino.IsInRaid())
            return false;

        if (raidDropZone == null)
            return false;

        if (RaidManager.Instance == null)
            return false;

        if (!raidDropZone.IsDinoInsideRaidZone(selectedDino))
            return false;

        RaidManager.Instance.AddDinoToRaid(selectedDino, selectedDinoStartPosition);
        return true;
    }

    private bool TryMergeSelectedDino()
    {
        if (selectedDino == null)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(selectedDino.transform.position, 0.6f);

        foreach (Collider2D hit in hits)
        {
            Dino other = hit.GetComponentInParent<Dino>();

            if (other == null)
                continue;

            if (other == selectedDino)
                continue;

            if (selectedDino.CanMergeWith(other))
            {
                GameManager.Instance.MergeDinos(selectedDino, other);
                return true;
            }
        }

        return false;
    }

    private Vector3 ClampPositionToField(Vector3 position)
    {
        if (GameManager.Instance == null)
            return position;

        if (CanDragToRaidZone(position))
            return position;

        Bounds bounds = GameManager.Instance.GetFieldBounds();

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return position;
    }

    private bool CanDragToRaidZone(Vector3 position)
    {
        if (selectedDino == null)
            return false;

        if (!selectedDino.CanGoToRaid())
            return false;

        if (raidDropZone == null)
            return false;

        return raidDropZone.ContainsPoint(position);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}
