using UnityEngine;
using UnityEngine.EventSystems;

public class DinoTouchInput : MonoBehaviour
{
    private Camera mainCamera;

    private Dino selectedDino;
    private Vector3 dragOffset;

    private bool isDragging;
    private Vector2 touchStartPosition;

    private const float dragDistance = 20f;

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
            if (IsPointerOverUI())
                return;

            TrySelectDino(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && selectedDino != null)
        {
            isDragging = true;
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
            if (IsPointerOverUI(touch.fingerId))
                return;

            touchStartPosition = touch.position;
            TrySelectDino(touch.position);
        }

        if (touch.phase == TouchPhase.Moved && selectedDino != null)
        {
            float distance = Vector2.Distance(touchStartPosition, touch.position);

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
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        Dino dino = hit.GetComponent<Dino>();

        if (dino == null)
            return;

        selectedDino = dino;
        dragOffset = selectedDino.transform.position - worldPosition;
        isDragging = false;

        selectedDino.SetDragging(true);
    }

    private void DragSelected(Vector2 screenPosition)
    {
        if (selectedDino == null)
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
        {
            bool merged = TryMergeSelectedDino();

            if (!merged)
            {
                // Нічого не робимо.
                // Дракончик просто залишається на новому місці.
            }
        }

        selectedDino = null;
        isDragging = false;
    }

    private bool TryMergeSelectedDino()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(selectedDino.transform.position, 0.6f);

        foreach (Collider2D hit in hits)
        {
            Dino other = hit.GetComponent<Dino>();

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