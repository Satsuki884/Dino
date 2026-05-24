using System.Collections.Generic;
using UnityEngine;

public class FoodInventory : MonoBehaviour
{
    public static FoodInventory Instance;

    [Header("Inventory UI")]
    public Transform foodSlotParent;
    public FoodUI foodSlotPrefab;

    [Header("Available Food")]
    public List<FoodConfig> availableFoods = new List<FoodConfig>();

    private readonly Dictionary<FoodConfig, int> foodAmounts = new Dictionary<FoodConfig, int>();
    private readonly List<FoodUI> spawnedSlots = new List<FoodUI>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food != null && !foodAmounts.ContainsKey(food))
                foodAmounts.Add(food, 0);
        }

        BuildInventoryUI();
    }

    public bool IsFoodUnlocked(FoodConfig food)
    {
        return food != null;
    }

    public void AddFood(FoodConfig food, int amount)
    {
        if (food == null)
            return;

        if (!foodAmounts.ContainsKey(food))
            foodAmounts.Add(food, 0);

        foodAmounts[food] += amount;
        RefreshInventoryUI();
    }

    public bool HasFood(FoodConfig food)
    {
        if (food == null)
            return false;

        return foodAmounts.ContainsKey(food) && foodAmounts[food] > 0;
    }

    public bool UseFood(FoodConfig food)
    {
        if (!HasFood(food))
            return false;

        foodAmounts[food]--;
        RefreshInventoryUI();
        return true;
    }

    public int GetFoodAmount(FoodConfig food)
    {
        if (food == null)
            return 0;

        if (!foodAmounts.ContainsKey(food))
            return 0;

        return foodAmounts[food];
    }

    private void BuildInventoryUI()
    {
        ClearInventoryUI();

        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food == null)
                continue;

            FoodUI slot = Instantiate(foodSlotPrefab, foodSlotParent);
            slot.Init(food);
            spawnedSlots.Add(slot);
        }

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        foreach (FoodUI slot in spawnedSlots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    private void ClearInventoryUI()
    {
        foreach (FoodUI slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }

    public List<FoodSaveData> GetSaveData()
    {
        List<FoodSaveData> data = new List<FoodSaveData>();

        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food == null)
                continue;

            FoodSaveData foodData = new FoodSaveData();
            foodData.foodName = food.foodName;
            foodData.amount = GetFoodAmount(food);

            data.Add(foodData);
        }

        return data;
    }

    public void LoadFromSave(List<FoodSaveData> savedFoods)
    {
        if (savedFoods == null)
            return;

        foodAmounts.Clear();

        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food != null && !foodAmounts.ContainsKey(food))
                foodAmounts.Add(food, 0);
        }

        foreach (FoodSaveData savedFood in savedFoods)
        {
            if (savedFood == null)
                continue;

            FoodConfig config = GetFoodByName(savedFood.foodName);

            if (config == null)
                continue;

            foodAmounts[config] = savedFood.amount;
        }

        RefreshInventoryUI();
    }

    private FoodConfig GetFoodByName(string foodName)
    {
        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food != null && food.foodName == foodName)
                return food;
        }

        return null;
    }

    private List<FoodConfig> GetConfiguredFoods()
    {
        if (availableFoods != null && availableFoods.Count > 0)
            return availableFoods;

        if (FoodShop.Instance != null)
            return FoodShop.Instance.availableFoods;

        return availableFoods;
    }
}
