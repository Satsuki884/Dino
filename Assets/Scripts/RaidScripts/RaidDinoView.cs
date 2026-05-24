using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaidDinoView : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Header("Timer UI")]
    public Slider timerSlider;
    public TMP_Text timerText;
    public TMP_Text levelText;

    [Header("Movement")]
    public float moveSpeed = 1f;

    private RaidEntry entry;
    private RaidController controller;

    private Bounds movementBounds;
    private Vector2 moveDirection;


    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(RaidEntry newEntry, RaidController newController, Bounds bounds)
    {
        entry = newEntry;
        controller = newController;
        movementBounds = bounds;

        moveDirection = Random.insideUnitCircle.normalized;

        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.right;

        Refresh();
    }

    private void Update()
    {
        Move();
        Refresh();
    }

    public void Click()
    {
        if (controller == null)
            return;

        controller.AskReturnDino(entry);
    }

    public void Refresh()
    {
        if (entry == null || entry.dino == null)
            return;

        if (spriteRenderer != null)
            spriteRenderer.sprite = entry.dino.GetCurrentSprite();

        if (levelText != null)
            levelText.text = "Lv." + entry.dino.Level + "  " + entry.dino.GetCurrentStageName();

        if (timerText != null)
            timerText.text = FormatTime(entry.timeLeft);

        if (timerSlider != null && RaidManager.Instance != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = 1f;
            timerSlider.value = Mathf.Clamp01(entry.timeLeft / entry.raidDuration);
        }
    }

    private void Move()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        Vector3 position = transform.position;

        bool changedDirection = false;

        if (position.x < movementBounds.min.x)
        {
            position.x = movementBounds.min.x;
            moveDirection.x = Mathf.Abs(moveDirection.x);
            changedDirection = true;
        }
        else if (position.x > movementBounds.max.x)
        {
            position.x = movementBounds.max.x;
            moveDirection.x = -Mathf.Abs(moveDirection.x);
            changedDirection = true;
        }

        if (position.y < movementBounds.min.y)
        {
            position.y = movementBounds.min.y;
            moveDirection.y = Mathf.Abs(moveDirection.y);
            changedDirection = true;
        }
        else if (position.y > movementBounds.max.y)
        {
            position.y = movementBounds.max.y;
            moveDirection.y = -Mathf.Abs(moveDirection.y);
            changedDirection = true;
        }

        transform.position = position;

        if (changedDirection)
            moveDirection = moveDirection.normalized;

        if (spriteRenderer != null && moveDirection.x != 0)
            spriteRenderer.flipX = moveDirection.x < 0;
    }

    private string FormatTime(float time)
    {
        time = Mathf.Max(0f, time);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}