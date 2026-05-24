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

    [Header("Shop Price Growth")]
    [Tooltip("Each bought egg of the same level multiplies the next price by this value.")]
    public float dinoEggPriceGrowthMultiplier = 1.18f;
    [Tooltip("Prices are rounded up to this step. Use 1 for no step rounding.")]
    public int dinoEggPriceRoundTo = 1;

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

    private readonly Dictionary<int, int> dinoEggPurchaseCounts = new Dictionary<int, int>();

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

        RefreshFoodUI();
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
        RefreshFoodUI();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;
        UpdateUI();
        RefreshFoodUI();
        return true;
    }

    private void RefreshFoodUI()
    {
        if (FoodShop.Instance != null)
            FoodShop.Instance.RefreshShopUI();

        if (FoodInventory.Instance != null)
            FoodInventory.Instance.RefreshInventoryUI();
    }

    public bool CanSpawnMoreDinos()
    {
        return activeDinos.Count < maxDinosOnField;
    }

    public bool BuyDino(int level)
    {
        if (!CanSpawnMoreDinos())
        {
            Debug.Log("Ліміт динозавриків на полі досягнуто.");
            return false;
        }

        DinoConfig config = GetConfigByLevel(level);

        if (config == null)
            return false;

        if (!IsDinoEggUnlockedInShop(level))
            return false;

        int currentPrice = GetDinoEggPrice(level);

        if (!SpendCoins(currentPrice))
            return false;

        Dino spawnedDino = SpawnDino(level, 1, GetRandomPointInField());

        if (spawnedDino == null)
        {
            coins += currentPrice;
            UpdateUI();
            return false;
        }

        RegisterDinoEggPurchase(level);

        if (shopManager != null)
            shopManager.RefreshShop();

        return true;
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

            RefreshFoodUI();
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

    public bool IsDinoEggUnlockedInShop(int eggLevel)
    {
        if (eggLevel <= 1)
            return true;

        return highestUnlockedLevel >= GetRequiredDinoLevelForShopEgg(eggLevel);
    }

    public int GetRequiredDinoLevelForShopEgg(int eggLevel)
    {
        if (eggLevel <= 1)
            return 1;

        return eggLevel + 2;
    }

    public int GetDinoEggPrice(int level)
    {
        DinoConfig config = GetConfigByLevel(level);

        if (config == null)
            return 0;

        int purchaseCount = GetDinoEggPurchaseCount(level);
        float multiplier = Mathf.Max(1f, dinoEggPriceGrowthMultiplier);
        float rawPrice = config.buyPrice * Mathf.Pow(multiplier, purchaseCount);

        return RoundPriceUp(rawPrice);
    }

    private int GetDinoEggPurchaseCount(int level)
    {
        if (!dinoEggPurchaseCounts.TryGetValue(level, out int purchaseCount))
            return 0;

        return Mathf.Max(0, purchaseCount);
    }

    private void RegisterDinoEggPurchase(int level)
    {
        int purchaseCount = GetDinoEggPurchaseCount(level);
        dinoEggPurchaseCounts[level] = purchaseCount + 1;
    }

    private int RoundPriceUp(float price)
    {
        int roundTo = Mathf.Max(1, dinoEggPriceRoundTo);
        int wholePrice = Mathf.Max(1, Mathf.CeilToInt(price));

        if (roundTo <= 1)
            return wholePrice;

        return Mathf.CeilToInt(wholePrice / (float)roundTo) * roundTo;
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
        data.dinoEggPrices = GetDinoEggPriceSaveData();

        if (FoodShop.Instance != null)
            data.selectedFoodBuyAmount = FoodShop.Instance.SelectedBuyAmount;

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
        LoadDinoEggPriceData(data.dinoEggPrices);

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

        if (FoodShop.Instance != null)
            FoodShop.Instance.SetSelectedBuyAmount(data.selectedFoodBuyAmount);

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

    private List<DinoEggPriceSaveData> GetDinoEggPriceSaveData()
    {
        List<DinoEggPriceSaveData> saveData = new List<DinoEggPriceSaveData>();

        foreach (KeyValuePair<int, int> pair in dinoEggPurchaseCounts)
        {
            DinoEggPriceSaveData priceData = new DinoEggPriceSaveData();
            priceData.level = pair.Key;
            priceData.purchaseCount = Mathf.Max(0, pair.Value);

            saveData.Add(priceData);
        }

        return saveData;
    }

    private void LoadDinoEggPriceData(List<DinoEggPriceSaveData> saveData)
    {
        dinoEggPurchaseCounts.Clear();

        if (saveData == null)
            return;

        foreach (DinoEggPriceSaveData priceData in saveData)
        {
            if (priceData == null)
                continue;

            if (priceData.level <= 0)
                continue;

            dinoEggPurchaseCounts[priceData.level] = Mathf.Max(0, priceData.purchaseCount);
        }
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
