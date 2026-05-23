using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DinoTouchInputNew : MonoBehaviour
{
    [Header("UI Blocking")]
    public bool blockInputOverUI = true;

    [Header("Drag Settings")]
    public float dragDistance = 5f;
    public float mergeRadius = 0.6f;

    private Camera mainCamera;

    private Dino selectedDino;
    private Vector3 dragOffset;

    private bool isDragging;
    private Vector2 pointerStartPosition;

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

        // Миша для ПК
        if (Mouse.current != null)
        {
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            isPressed = Mouse.current.leftButton.isPressed;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            screenPosition = Mouse.current.position.ReadValue();
        }

        // Палець для телефона
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

        if (pressedThisFrame)
        {
            if (blockInputOverUI && IsPointerOverUI())
                return;

            pointerStartPosition = screenPosition;
            TrySelectDino(screenPosition);
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

    private void TrySelectDino(Vector2 screenPosition)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Check MainCamera tag.");
            return;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        /*
         Важливо:
         Використовуємо OverlapPointAll, а не OverlapPoint.
         Так ми не ламаємось, якщо першим знайдеться Field.
         Ми просто перебираємо всі колайдери під курсором і беремо той,
         у якого є Dino в батьківських об'єктах.
        */
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        if (hits == null || hits.Length == 0)
            return;

        foreach (Collider2D hit in hits)
        {
            Dino dino = hit.GetComponentInParent<Dino>();

            if (dino == null)
                continue;

            selectedDino = dino;
            dragOffset = selectedDino.transform.position - worldPosition;
            isDragging = false;

            selectedDino.SetDragging(true);

            return;
        }
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

        if (isDragging)
            TryMergeSelectedDino();

        selectedDino = null;
        isDragging = false;
    }

    private bool TryMergeSelectedDino()
    {
        if (selectedDino == null)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            selectedDino.transform.position,
            mergeRadius
        );

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

        Bounds bounds = GameManager.Instance.GetFieldBounds();

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

        return position;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}