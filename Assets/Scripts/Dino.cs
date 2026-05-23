using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dino : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Header("UI Above Head")]
    public Slider caloriesSlider;
    public Slider growthSlider;
    public TMP_Text levelText;

    [Header("Runtime Info")]
    [SerializeField] private DinoConfig config;
    [SerializeField] private int currentStage = 1;
    [SerializeField] private float calories;
    [SerializeField] private float growthTicks;

    [Header("Dropped Coins")]
    public Vector2 droppedCoinRandomOffset = new Vector2(0.7f, 0.5f);

    private float droppedCoinTimer;
    private float nextDroppedCoinTime;

    private Vector2 moveDirection;
    private bool isDragging;
    private float tickTimer;
    private bool isInRaid;

    public int Level => config != null ? config.level : 0;
    public int Stage => currentStage;

    private DinoStageData CurrentStageData
    {
        get
        {
            if (config == null)
                return null;

            if (config.stages == null || config.stages.Length == 0)
                return null;

            int index = Mathf.Clamp(currentStage - 1, 0, config.stages.Length - 1);
            return config.stages[index];
        }
    }

    private void Update()
    {
        if (config == null)
            return;

        if (isInRaid)
            return;

        if (!isDragging && CanMoveByStage())
            Move();

        HandleTick();
        HandleDroppedCoinSpawn();

        UpdateUI();
    }
    private bool CanMoveByStage()
    {
        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return false;

        return stageData.canMove;
    }

    public void Init(DinoConfig newConfig, int startStage)
    {
        config = newConfig;
        currentStage = Mathf.Max(1, startStage);

        calories = config.startCalories;
        growthTicks = 0f;
        tickTimer = 0f;

        moveDirection = Random.insideUnitCircle.normalized;

        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.right;

        ScheduleNextDroppedCoin();
        UpdateVisual();
        UpdateUI();
    }

    private void ScheduleNextDroppedCoin()
    {
        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
        {
            nextDroppedCoinTime = 5f;
            droppedCoinTimer = 0f;
            return;
        }

        float min = Mathf.Max(0.5f, stageData.spawnedCoinIntervalMin);
        float max = Mathf.Max(min, stageData.spawnedCoinIntervalMax);

        nextDroppedCoinTime = Random.Range(min, max);
        droppedCoinTimer = 0f;
    }

    private void HandleDroppedCoinSpawn()
    {
        if (!HasCalories())
            return;

        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return;

        droppedCoinTimer += Time.deltaTime;

        if (droppedCoinTimer < nextDroppedCoinTime)
            return;

        droppedCoinTimer = 0f;

        TrySpawnDroppedCoin(stageData);
        ScheduleNextDroppedCoin();
    }

    private void TrySpawnDroppedCoin(DinoStageData stageData)
    {
        if (GameManager.Instance == null)
            return;

        int maxCoinValue = Mathf.FloorToInt(stageData.coinsPerTick);

        if (maxCoinValue <= 0)
            return;

        int coinValue = Random.Range(0, maxCoinValue + 1);

        if (coinValue <= 0)
            return;

        Vector3 offset = new Vector3(
            Random.Range(-droppedCoinRandomOffset.x, droppedCoinRandomOffset.x),
            Random.Range(-droppedCoinRandomOffset.y, droppedCoinRandomOffset.y),
            0f
        );

        Vector3 spawnPosition = transform.position + offset;

        GameManager.Instance.SpawnDroppedCoin(spawnPosition, coinValue);
    }

    private bool HasCalories()
    {
        return calories > 0f;
    }

    public void SetDragging(bool value)
    {
        isDragging = value;
    }

    private void HandleTick()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer < 1f)
            return;

        int ticksPassed = Mathf.FloorToInt(tickTimer);
        tickTimer -= ticksPassed;

        for (int i = 0; i < ticksPassed; i++)
        {
            ProcessOneTick();
        }
    }

    private void ProcessOneTick()
    {
        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return;

        if (calories <= 0f)
        {
            calories = 0f;
            return;
        }

        calories -= stageData.caloriesConsumePerTick;

        if (calories < 0f)
            calories = 0f;

        AddCoinsForCurrentStage(stageData);

        AddGrowthForCurrentStage(stageData);
    }

    private void AddCoinsForCurrentStage(DinoStageData stageData)
    {
        if (GameManager.Instance == null)
            return;

        if (stageData.coinsPerTick <= 0f)
            return;

        GameManager.Instance.AddCoins(stageData.coinsPerTick);
    }

    private void AddGrowthForCurrentStage(DinoStageData stageData)
    {
        if (IsFinalStage())
            return;

        if (stageData.ticksToNextStage <= 0f)
            return;

        float multiplier = 1f;

        if (config != null)
            multiplier = Mathf.Max(1f, config.growthTimeMultiplier);

        float requiredTicks = stageData.ticksToNextStage * multiplier;

        growthTicks += 1f;

        if (growthTicks >= requiredTicks)
        {
            growthTicks = 0f;
            TryGrowToNextStage();
        }
    }

    public bool Feed(FoodConfig food)
    {
        if (isInRaid)
            return false;

        if (food == null)
            return false;

        calories += food.satietyValue;

        if (food.bonusGrowthExperience > 0f && !IsFinalStage())
        {
            growthTicks += food.bonusGrowthExperience;

            DinoStageData stageData = CurrentStageData;

            while (stageData != null &&
                   !IsFinalStage() &&
                   stageData.ticksToNextStage > 0f &&
                   growthTicks >= stageData.ticksToNextStage)
            {
                growthTicks -= stageData.ticksToNextStage;
                TryGrowToNextStage();
                stageData = CurrentStageData;
            }
        }

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

    public bool CanMergeWith(Dino other)
    {
        if (other == null)
            return false;

        if (other == this)
            return false;

        if (isInRaid || other.IsInRaid())
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

        if (config.stages == null)
            return false;

        if (config.stages.Length == 0)
            return false;

        return currentStage >= config.stages.Length;
    }

    public float GetCurrentCoinsPerTick()
    {
        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return 0f;

        return stageData.coinsPerTick;
    }

    public string GetCurrentStageName()
    {
        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return "Stage " + currentStage;

        return stageData.stageName;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return;

        spriteRenderer.sprite = stageData.sprite;
    }

    private void UpdateUI()
    {
        UpdateCaloriesUI();
        UpdateGrowthUI();
        UpdateLevelText();
    }

    private void UpdateCaloriesUI()
    {
        if (caloriesSlider == null)
            return;

        bool shouldShowCaloriesBar = calories <= 0f;

        caloriesSlider.gameObject.SetActive(shouldShowCaloriesBar);

        if (shouldShowCaloriesBar)
            caloriesSlider.value = 0f;
    }

    private void UpdateGrowthUI()
    {
        if (growthSlider == null)
            return;

        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
        {
            growthSlider.value = 0f;
            return;
        }

        if (IsFinalStage())
        {
            growthSlider.gameObject.SetActive(false);
            return;
        }

        growthSlider.gameObject.SetActive(true);

        if (stageData.ticksToNextStage <= 0f)
        {
            growthSlider.value = 0f;
            return;
        }

        float multiplier = config != null ? Mathf.Max(1f, config.growthTimeMultiplier) : 1f;
        float requiredTicks = stageData.ticksToNextStage * multiplier;

        growthSlider.value = growthTicks / requiredTicks;
    }

    private void UpdateLevelText()
    {
        if (levelText == null)
            return;

        if (IsMaxDino())
        {
            levelText.text = "MAX";
        }
        else
        {
            levelText.text = "Lv." + Level + "  " + GetCurrentStageName();
        }
    }

    public bool IsMaxDino()
    {
        if (!IsFinalStage())
            return false;

        if (GameManager.Instance == null)
            return false;

        return GameManager.Instance.IsMaxDinoLevel(Level);
    }

    public bool IsInRaid()
    {
        return isInRaid;
    }

    public void StartRaidMode()
    {
        isInRaid = true;
        isDragging = false;

        gameObject.SetActive(false);
    }

    public void EndRaidMode(Vector3 returnPosition)
    {
        transform.position = returnPosition;

        calories = 0f;

        gameObject.SetActive(true);

        isInRaid = false;

        UpdateVisual();
        UpdateUI();
    }

    public Sprite GetCurrentSprite()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            return spriteRenderer.sprite;

        DinoStageData stageData = CurrentStageData;

        if (stageData == null)
            return null;

        return stageData.sprite;
    }
}