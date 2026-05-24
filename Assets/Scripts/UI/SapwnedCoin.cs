using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SpawnedCoin : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text valueText;

    [Header("Settings")]
    public float lifeTime = 5f;

    private int value;
    private bool collected;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Init(int coinValue, float newLifeTime = 5f)
    {
        value = coinValue;
        lifeTime = newLifeTime;

        if (valueText != null)
            valueText.text = CoinFormatter.FormatNumber(value);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        HandleClickOrTouch();
    }

    private void HandleClickOrTouch()
    {
        if (collected)
            return;

        bool pressed = false;
        Vector2 screenPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressed = true;
            screenPosition = Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            pressed = true;
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            pressed = true;
            screenPosition = Input.mousePosition;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pressed = true;
            screenPosition = Input.GetTouch(0).position;
        }
#endif

        if (!pressed)
            return;

        TryCollectAtScreenPosition(screenPosition);
    }

    private void TryCollectAtScreenPosition(Vector2 screenPosition)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Collect();
                return;
            }
        }
    }

    private void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(value);

        Destroy(gameObject);
    }

}
