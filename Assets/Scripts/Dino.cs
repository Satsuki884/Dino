using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dino : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Header("UI Above Head")]
    public Slider satietySlider;
    public Slider growthSlider;
    public TMP_Text levelText;

    [Header("Runtime Info")]
    [SerializeField] private DinoConfig config;
    [SerializeField] private int currentStage = 1;
    [SerializeField] private float satiety;
    [SerializeField] private float growthExperience;

    private Vector2 moveDirection;
    private bool isDragging;
    private Camera mainCamera;
    private Vector3 dragOffset;

    private float coinTimer;

    public int Level => config.level;
    public int Stage => currentStage;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Init(DinoConfig newConfig, int startStage)
    {
        config = newConfig;
        currentStage = startStage;

        satiety = config.maxSatiety;
        growthExperience = 0f;

        moveDirection = Random.insideUnitCircle.normalized;

        UpdateVisual();
        UpdateUI();
    }

    private void Update()
    {
        if (config == null)
            return;

        LoseSatiety();

        if (!isDragging)
            Move();

        HandleGrowth();
        HandleCoins();

        UpdateUI();
    }

    private void LoseSatiety()
    {
        satiety -= config.satietyLossPerSecond * Time.deltaTime;
        satiety = Mathf.Clamp(satiety, 0f, config.maxSatiety);
    }

    private void Move()
    {
        transform.position += (Vector3)(moveDirection * config.moveSpeed * Time.deltaTime);

        Bounds bounds = GameManager.Instance.GetFieldBounds();
        Vector3 position = transform.position;

        bool changedDirection = false;

        if (position.x < bounds.min.x)
        {
            position.x = bounds.min.x;
            moveDirection.x = Mathf.Abs(moveDirection.x);
            changedDirection = true;
        }
        else if (position.x > bounds.max.x)
        {
            position.x = bounds.max.x;
            moveDirection.x = -Mathf.Abs(moveDirection.x);
            changedDirection = true;
        }

        if (position.y < bounds.min.y)
        {
            position.y = bounds.min.y;
            moveDirection.y = Mathf.Abs(moveDirection.y);
            changedDirection = true;
        }
        else if (position.y > bounds.max.y)
        {
            position.y = bounds.max.y;
            moveDirection.y = -Mathf.Abs(moveDirection.y);
            changedDirection = true;
        }

        transform.position = position;

        if (changedDirection)
            moveDirection = moveDirection.normalized;

        if (moveDirection.x != 0)
            spriteRenderer.flipX = moveDirection.x < 0;
    }

    private void HandleGrowth()
    {
        if (IsFinalStage())
            return;

        if (GetSatietyPercent() < config.minSatietyForGrowth)
            return;

        growthExperience += config.experiencePerSecondWhenFed * Time.deltaTime;

        if (growthExperience >= config.growthExperienceToNextStage)
        {
            TryGrowToNextStage();
        }
    }

    private void TryGrowToNextStage()
    {
        if (IsFinalStage())
            return;

        growthExperience = 0f;
        currentStage++;

        UpdateVisual();
        UpdateUI();

        GameManager.Instance.RegisterDiscoveredStage(config, currentStage);
    }

    private void HandleCoins()
    {
        if (GetSatietyPercent() < config.minSatietyForGrowth)
            return;

        coinTimer += Time.deltaTime;

        if (coinTimer >= 1f)
        {
            coinTimer = 0f;
            GameManager.Instance.AddCoins(config.coinsPerSecond);
        }
    }

    public bool Feed(FoodConfig food)
{
    if (food == null)
        return false;

    satiety += food.satietyValue;
    satiety = Mathf.Clamp(satiety, 0f, config.maxSatiety);

    growthExperience += food.bonusGrowthExperience;

    if (growthExperience >= config.growthExperienceToNextStage)
    {
        TryGrowToNextStage();
    }

    UpdateUI();

    return true;
}

    public bool CanMergeWith(Dino other)
    {
        if (other == null)
            return false;

        if (Level != other.Level)
            return false;

        if (!IsFinalStage())
            return false;

        if (!other.IsFinalStage())
            return false;

        return true;
    }

    public bool IsFinalStage()
    {
        if (config == null)
            return false;

        if (config.stageSprites == null)
            return false;

        return currentStage >= config.stageSprites.Length;
    }

    private float GetSatietyPercent()
    {
        return satiety / config.maxSatiety * 100f;
    }

    private void UpdateVisual()
    {
        if (config.stageSprites == null || config.stageSprites.Length == 0)
            return;

        int spriteIndex = Mathf.Clamp(currentStage - 1, 0, config.stageSprites.Length - 1);

        if (spriteRenderer != null)
            spriteRenderer.sprite = config.stageSprites[spriteIndex];
    }

    private void UpdateUI()
    {
        if (satietySlider != null)
            satietySlider.value = satiety / config.maxSatiety;

        if (growthSlider != null)
        {
            if (IsFinalStage())
                growthSlider.value = 1f;
            else
                growthSlider.value = growthExperience / config.growthExperienceToNextStage;
        }

        if (levelText != null)
            levelText.text = "Lv." + Level + "  St." + currentStage;
    }
/*
    private void OnMouseDown()
    {
        StartDrag();
    }

    private void OnMouseDrag()
    {
        Drag();
    }

    private void OnMouseUp()
    {
        EndDrag();
    }

    private void StartDrag()
    {
        isDragging = true;

        Vector3 mouseWorld = GetPointerWorldPosition();
        dragOffset = transform.position - mouseWorld;
    }

    private void Drag()
    {
        if (!isDragging)
            return;

        Vector3 pointerWorld = GetPointerWorldPosition();
        Vector3 targetPosition = pointerWorld + dragOffset;
        targetPosition.z = 0f;

        transform.position = targetPosition;
    }

    private void EndDrag()
    {
        isDragging = false;

        Dino target = FindMergeTarget();

        if (target != null)
        {
            GameManager.Instance.MergeDinos(this, target);
        }
    } */

    private Dino FindMergeTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);

        foreach (Collider2D hit in hits)
        {
            Dino other = hit.GetComponent<Dino>();

            if (other == null)
                continue;

            if (other == this)
                continue;

            if (CanMergeWith(other))
                return other;
        }

        return null;
    }

    private Vector3 GetPointerWorldPosition()
    {
        Vector3 screenPosition;

        if (Input.touchCount > 0)
            screenPosition = Input.GetTouch(0).position;
        else
            screenPosition = Input.mousePosition;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        return worldPosition;
    }

    public void SetDragging(bool value)
    {
        isDragging = value;
    }
}