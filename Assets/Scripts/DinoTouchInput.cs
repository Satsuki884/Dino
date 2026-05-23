using UnityEngine;
using UnityEngine.EventSystems;

public class DinoTouchInput : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask dinoLayerMask;

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
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Check MainCamera tag.");
            return;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition, dinoLayerMask);

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("No Dino collider under pointer.");
            return;
        }

        foreach (Collider2D hit in hits)
        {
            Dino dino = hit.GetComponentInParent<Dino>();

            if (dino == null)
                continue;

            selectedDino = dino;
            dragOffset = selectedDino.transform.position - worldPosition;
            isDragging = false;

            selectedDino.SetDragging(true);

            Debug.Log("Selected dino: " + selectedDino.name);

            return;
        }

        Debug.Log("Dino layer collider found, but Dino component not found.");
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
        {
            TryMergeSelectedDino();
        }

        selectedDino = null;
        isDragging = false;
    }

    private bool TryMergeSelectedDino()
    {
        if (selectedDino == null)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            selectedDino.transform.position,
            0.6f,
            dinoLayerMask
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