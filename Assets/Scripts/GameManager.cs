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
    [Header("Spawned Coins")]
    public SpawnedCoin spawnedCoinPrefab;
    public Transform spawnedCoinParent;

    public void SpawnDroppedCoin(Vector3 position, int value)
    {
        if (value <= 0)
            return;

        if (spawnedCoinPrefab == null)
        {
            Debug.LogWarning("Spawned Coin Prefab is not assigned in GameManager.");
            return;
        }

        Transform parent = spawnedCoinParent != null ? spawnedCoinParent : null;

        SpawnedCoin coin = Instantiate(spawnedCoinPrefab, position, Quaternion.identity, parent);
        coin.Init(value, 5f);
    }

    private readonly List<Dino> activeDinos = new List<Dino>();
    public IReadOnlyList<Dino> ActiveDinos => activeDinos;

    private int highestUnlockedLevel = 1;

    private readonly HashSet<string> discoveredDinoStages = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (SaveManager.HasSave())
        {
            LoadGame();
        }
        else
        {
            SpawnDino(1, 1, GetRandomPointInField());
            UpdateUI();
        }

        if (shopManager != null)
        {
            shopManager.BuildShop();
            shopManager.RefreshShop();
        }

        if (FoodInventory.Instance != null)
            FoodInventory.Instance.RefreshInventoryUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private float coinRemainder = 0f;

    public void AddCoins(float amount)
    {
        if (amount <= 0f)
            return;

        coinRemainder += amount;

        int wholeCoins = Mathf.FloorToInt(coinRemainder);

        if (wholeCoins <= 0)
            return;

        coins += wholeCoins;
        coinRemainder -= wholeCoins;

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

        if (!HasConfigForLevel(newLevel))
        {
            Debug.Log("Cannot merge dinos: no DinoConfig for level " + newLevel + ".");
            return;
        }

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

            if (FoodInventory.Instance != null)
                FoodInventory.Instance.RefreshInventoryUI();
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
            coinsText.text = "Coins: " + CoinFormatter.FormatNumber(coins);

        if (dinoLimitText != null)
            dinoLimitText.text = activeDinos.Count + " / " + maxDinosOnField;
    }

    public bool HasConfigForLevel(int level)
    {
        return GetConfigByLevel(level) != null;
    }

    public int GetMaxDinoLevel()
    {
        int maxLevel = 0;

        foreach (DinoConfig config in dinoConfigs)
        {
            if (config != null && config.level > maxLevel)
                maxLevel = config.level;
        }

        return maxLevel;
    }

    public bool IsMaxDinoLevel(int level)
    {
        return level >= GetMaxDinoLevel();
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        data.coins = coins;
        data.highestUnlockedLevel = highestUnlockedLevel;

        foreach (Dino dino in activeDinos)
        {
            if (dino == null)
                continue;

            DinoSaveData dinoData = dino.GetSaveData();

            if (RaidManager.Instance != null &&
                RaidManager.Instance.TryGetRaidEntryForDino(dino, out RaidEntry raidEntry))
            {
                dinoData.isInRaid = true;

                dinoData.raidTimeLeft = raidEntry.timeLeft;
                dinoData.raidDuration = raidEntry.raidDuration;
                dinoData.raidRewardCoins = raidEntry.rewardCoins;

                dinoData.raidReturnPositionX = raidEntry.returnPosition.x;
                dinoData.raidReturnPositionY = raidEntry.returnPosition.y;
                dinoData.raidReturnPositionZ = raidEntry.returnPosition.z;
            }
            else
            {
                dinoData.isInRaid = false;
            }

            data.dinos.Add(dinoData);
        }

        if (FoodInventory.Instance != null)
            data.foods = FoodInventory.Instance.GetSaveData();

        SaveManager.Save(data);
    }

    public void LoadGame()
    {
        GameSaveData data = SaveManager.Load();

        if (data == null)
        {
            SpawnDino(1, 1, GetRandomPointInField());
            UpdateUI();
            return;
        }

        long currentUnixTime = SaveManager.GetCurrentUnixTime();
        float offlineSeconds = Mathf.Max(0f, currentUnixTime - data.lastSaveUnixTime);

        coins = data.coins;
        highestUnlockedLevel = Mathf.Max(1, data.highestUnlockedLevel);

        ClearAllDinos();

        foreach (DinoSaveData dinoData in data.dinos)
        {
            if (dinoData == null)
                continue;

            DinoConfig config = GetConfigByLevel(dinoData.level);

            if (config == null)
                continue;

            Vector3 position = new Vector3(
                dinoData.positionX,
                dinoData.positionY,
                dinoData.positionZ
            );

            if (dinoData.isInRaid)
            {
                position = new Vector3(
                    dinoData.raidReturnPositionX,
                    dinoData.raidReturnPositionY,
                    dinoData.raidReturnPositionZ
                );
            }

            Dino dino = Instantiate(dinoPrefab, position, Quaternion.identity, dinoParent);
            dino.LoadFromSave(config, dinoData);
            activeDinos.Add(dino);

            if (dinoData.isInRaid && RaidManager.Instance != null)
            {
                Vector3 returnPosition = new Vector3(
                    dinoData.raidReturnPositionX,
                    dinoData.raidReturnPositionY,
                    dinoData.raidReturnPositionZ
                );

                RaidManager.Instance.RestoreRaidDino(
                    dino,
                    returnPosition,
                    dinoData.raidTimeLeft,
                    dinoData.raidDuration,
                    dinoData.raidRewardCoins,
                    offlineSeconds
                );
            }
        }

        if (activeDinos.Count == 0)
            SpawnDino(1, 1, GetRandomPointInField());

        if (FoodInventory.Instance != null)
            FoodInventory.Instance.LoadFromSave(data.foods);

        UpdateUI();

        if (shopManager != null)
            shopManager.RefreshShop();
    }

    private void ClearAllDinos()
    {
        foreach (Dino dino in activeDinos)
        {
            if (dino != null)
                Destroy(dino.gameObject);
        }

        activeDinos.Clear();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGame();
    }

    [ContextMenu("Delete Save")]
    public void DeleteSaveForTest()
    {
        SaveManager.DeleteSave();
    }
}
