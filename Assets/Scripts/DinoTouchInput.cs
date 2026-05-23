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

    private Camera mainCamera;

    private Dino selectedDino;
    private Vector3 dragOffset;

    private bool isDragging;
    private Vector2 pointerStartPosition;

    private const float dragDistance = 5f;

    private void Awake()
    {
        mainCamera = Camera.main;}

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

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            TouchControl touch = Touchscreen.current.primaryTouch;

            pressedThisFrame = touch.press.wasPressedThisFrame;
            isPressed = touch.press.isPressed;
            releasedThisFrame = touch.press.wasReleasedThisFrame;
            screenPosition = touch.position.ReadValue();
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
            if (blockInputOverUI && IsPointerOverUI())
            {
                return;
            }

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
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
        {
            return;
        }

        Dino dino = hit.GetComponentInParent<Dino>();

        if (dino == null)
        {
            return;
        }

        selectedDino = dino;
        dragOffset = selectedDino.transform.position - worldPosition;
        isDragging = false;

        selectedDino.SetDragging(true);
    }

    private void DragSelected(Vector2 screenPosition)
    {
        if (selectedDino == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

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