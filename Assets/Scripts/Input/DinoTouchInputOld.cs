using UnityEngine;
using UnityEngine.EventSystems;

public class DinoTouchInputOld : MonoBehaviour
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
        HandleMouseInput();
        HandleTouchInput();
    }

    private void HandleMouseInput()
    {
        if (Input.touchCount > 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (blockInputOverUI && IsPointerOverUI())
                return;

            pointerStartPosition = Input.mousePosition;
            TrySelectDino(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && selectedDino != null)
        {
            float distance = Vector2.Distance(pointerStartPosition, Input.mousePosition);

            if (distance > dragDistance)
                isDragging = true;

            if (isDragging)
                DragSelected(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseSelected();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (blockInputOverUI && IsPointerOverUI(touch.fingerId))
                return;

            pointerStartPosition = touch.position;
            TrySelectDino(touch.position);
        }

        if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && selectedDino != null)
        {
            float distance = Vector2.Distance(pointerStartPosition, touch.position);

            if (distance > dragDistance)
                isDragging = true;

            if (isDragging)
                DragSelected(touch.position);
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
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
         Беремо всі Collider2D під курсором, а не перший.
         Так Field не заважає, бо ми вибираємо тільки той об'єкт,
         у якого є компонент Dino у батьківських об'єктах.
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

    private bool IsPointerOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }
}