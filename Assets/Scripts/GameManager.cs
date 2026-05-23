using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Resources")]
    public int coins = 100;

    [Header("Dino Limit")]
    public int maxDinosOnField = 10;

    [Header("Dino")]
    public Dino dinoPrefab;
    public Transform dinoParent;
    public List<DinoConfig> dinoConfigs = new List<DinoConfig>();

    [Header("Field")]
    public BoxCollider2D fieldBounds;

    [Header("UI")]
    public TMP_Text coinsText;
    public TMP_Text dinoLimitText;

    [Header("Shop")]
    public ShopManager shopManager;

    private readonly List<Dino> activeDinos = new List<Dino>();

    private int highestUnlockedLevel = 1;

    private readonly HashSet<string> discoveredDinoStages = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnDino(1, 1, GetRandomPointInField());
        UpdateUI();

        if (shopManager != null)
            shopManager.BuildShop();
    }

    private void Update()
    {
        UpdateUI();
    }

    public void AddCoins(float amount)
    {
        coins += Mathf.FloorToInt(amount);
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;
        UpdateUI();
        return true;
    }

    public bool CanSpawnMoreDinos()
    {
        return activeDinos.Count < maxDinosOnField;
    }

    public void BuyDino(int level)
    {
        if (!CanSpawnMoreDinos())
        {
            Debug.Log("Ліміт динозавриків на полі досягнуто.");
            return;
        }

        DinoConfig config = GetConfigByLevel(level);

        if (config == null)
            return;

        if (level > highestUnlockedLevel)
            return;

        if (!SpendCoins(config.buyPrice))
            return;

        SpawnDino(level, 1, GetRandomPointInField());
    }

    public Dino SpawnDino(int level, int stage, Vector3 position)
    {
        if (!CanSpawnMoreDinos())
        {
            Debug.Log("Не можна створити динозаврика: ліміт поля.");
            return null;
        }

        DinoConfig config = GetConfigByLevel(level);

        if (config == null)
        {
            Debug.LogError("No DinoConfig for level: " + level);
            return null;
        }

        Dino dino = Instantiate(dinoPrefab, position, Quaternion.identity, dinoParent);
        dino.Init(config, stage);
        activeDinos.Add(dino);

        UnlockLevel(level);
        RegisterDiscoveredStage(config, stage);

        UpdateUI();

        return dino;
    }

    public void RemoveDino(Dino dino)
    {
        if (activeDinos.Contains(dino))
            activeDinos.Remove(dino);

        Destroy(dino.gameObject);
        UpdateUI();
    }

    public void MergeDinos(Dino first, Dino second)
    {
        if (first == null || second == null)
            return;

        if (first == second)
            return;

        if (!first.CanMergeWith(second))
            return;

        int newLevel = first.Level + 1;
        Vector3 spawnPosition = (first.transform.position + second.transform.position) / 2f;

        RemoveDino(first);
        RemoveDino(second);

        SpawnDino(newLevel, 1, spawnPosition);

        UnlockLevel(newLevel);
        UpdateUI();
    }

    private void UnlockLevel(int level)
    {
        if (level > highestUnlockedLevel)
        {
            highestUnlockedLevel = level;

            if (shopManager != null)
                shopManager.RefreshShop();
        }
    }

    public void RegisterDiscoveredStage(DinoConfig config, int stage)
    {
        if (config == null)
            return;

        string key = GetDiscoveryKey(config.level, stage);

        if (discoveredDinoStages.Contains(key))
            return;

        discoveredDinoStages.Add(key);

        if (DiscoveryPopupUI.Instance != null)
            DiscoveryPopupUI.Instance.Show(config, stage);
    }

    private string GetDiscoveryKey(int level, int stage)
    {
        return "Dino_Level_" + level + "_Stage_" + stage;
    }

    public bool IsLevelUnlocked(int level)
    {
        return level <= highestUnlockedLevel;
    }

    public DinoConfig GetConfigByLevel(int level)
    {
        foreach (DinoConfig config in dinoConfigs)
        {
            if (config.level == level)
                return config;
        }

        return null;
    }

    public Vector3 GetRandomPointInField()
    {
        Bounds bounds = fieldBounds.bounds;

        float x = Random.Range(bounds.min.x + 1f, bounds.max.x - 1f);
        float y = Random.Range(bounds.min.y + 1f, bounds.max.y - 1f);

        return new Vector3(x, y, 0f);
    }

    public Bounds GetFieldBounds()
    {
        return fieldBounds.bounds;
    }

    public int GetDinoCount()
    {
        return activeDinos.Count;
    }

    private void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = "Coins: " + coins;

        if (dinoLimitText != null)
            dinoLimitText.text = activeDinos.Count + " / " + maxDinosOnField;
    }
}