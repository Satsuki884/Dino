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
    private float coinTimer;

    public int Level => config != null ? config.level : 0;
    public int Stage => currentStage;

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

    public void Init(DinoConfig newConfig, int startStage)
    {
        config = newConfig;
        currentStage = startStage;

        satiety = config.startSatiety;
        growthExperience = 0f;
        coinTimer = 0f;

        moveDirection = Random.insideUnitCircle.normalized;

        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.right;

        UpdateVisual();
        UpdateUI();
    }

    public void SetDragging(bool value)
    {
        isDragging = value;
    }

    private void LoseSatiety()
    {
        if (satiety <= 0f)
        {
            satiety = 0f;
            return;
        }

        satiety -= config.satietyLossPerSecond * Time.deltaTime;

        if (satiety < 0f)
            satiety = 0f;
    }

    private bool HasSatiety()
    {
        return satiety > 0f;
    }

    private void Move()
    {
        if (GameManager.Instance == null)
            return;

        transform.position += (Vector3)(moveDirection * config.moveSpeed * Time.deltaTime);

        Bounds bounds = GameManager.Instance.GetFieldBounds();
        Vector3 position = transform.position;

        if (position.x < bounds.min.x)
        {
            position.x = bounds.min.x;
            moveDirection.x = Mathf.Abs(moveDirection.x);
        }
        else if (position.x > bounds.max.x)
        {
            position.x = bounds.max.x;
            moveDirection.x = -Mathf.Abs(moveDirection.x);
        }

        if (position.y < bounds.min.y)
        {
            position.y = bounds.min.y;
            moveDirection.y = Mathf.Abs(moveDirection.y);
        }
        else if (position.y > bounds.max.y)
        {
            position.y = bounds.max.y;
            moveDirection.y = -Mathf.Abs(moveDirection.y);
        }

        transform.position = position;

        if (moveDirection != Vector2.zero)
            moveDirection.Normalize();

        if (spriteRenderer != null && moveDirection.x != 0)
            spriteRenderer.flipX = moveDirection.x < 0;
    }

    private void HandleGrowth()
    {
        if (IsFinalStage())
            return;

        if (!HasSatiety())
            return;

        growthExperience += config.experiencePerSecondWhenFed * Time.deltaTime;

        if (growthExperience >= config.growthExperienceToNextStage)
        {
            TryGrowToNextStage();
        }
    }

    private void HandleCoins()
    {
        if (!HasSatiety())
            return;

        coinTimer += Time.deltaTime;

        if (coinTimer >= 1f)
        {
            coinTimer = 0f;

            if (GameManager.Instance != null)
                GameManager.Instance.AddCoins(config.coinsPerSecond);
        }
    }

    public bool Feed(FoodConfig food)
    {
        if (food == null)
            return false;

        satiety += food.satietyValue;

        growthExperience += food.bonusGrowthExperience;

        while (growthExperience >= config.growthExperienceToNextStage && !IsFinalStage())
        {
            growthExperience -= config.growthExperienceToNextStage;
            TryGrowToNextStage();
        }

        if (IsFinalStage())
            growthExperience = config.growthExperienceToNextStage;

        UpdateUI();

        return true;
    }

    private void TryGrowToNextStage()
    {
        if (IsFinalStage())
            return;

        currentStage++;

        UpdateVisual();
        UpdateUI();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterDiscoveredStage(config, currentStage);
    }

    public bool CanMergeWith(Dino other)
    {
        if (other == null)
            return false;

        if (other == this)
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

        if (config.stageSprites.Length == 0)
            return false;

        return currentStage >= config.stageSprites.Length;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        if (config == null)
            return;

        if (config.stageSprites == null || config.stageSprites.Length == 0)
            return;

        int spriteIndex = Mathf.Clamp(currentStage - 1, 0, config.stageSprites.Length - 1);
        spriteRenderer.sprite = config.stageSprites[spriteIndex];
    }

    private void UpdateUI()
    {
        UpdateSatietyUI();
        UpdateGrowthUI();
        UpdateLevelText();
    }

    private void UpdateSatietyUI()
    {
        if (satietySlider == null)
            return;

        bool shouldShowSatietyBar = satiety <= 0f;

        satietySlider.gameObject.SetActive(shouldShowSatietyBar);

        if (shouldShowSatietyBar)
            satietySlider.value = 0f;
    }

    private void UpdateGrowthUI()
    {
        if (growthSlider == null)
            return;

        if (config == null)
        {
            growthSlider.value = 0f;
            return;
        }

        if (IsFinalStage())
        {
            growthSlider.value = 1f;
            return;
        }

        if (config.growthExperienceToNextStage <= 0f)
        {
            growthSlider.value = 0f;
            return;
        }

        growthSlider.value = growthExperience / config.growthExperienceToNextStage;
    }

    private void UpdateLevelText()
    {
        if (levelText == null)
            return;

        levelText.text = "Lv." + Level + "  St." + currentStage;
    }
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
