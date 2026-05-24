using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodInventory : MonoBehaviour
{
    public static FoodInventory Instance;

    [Header("Inventory UI")]
    public Transform foodSlotParent;
    public FoodSlotUI foodSlotPrefab;

    [Header("Available Food")]
    public List<FoodConfig> availableFoods = new List<FoodConfig>();

    [Header("Buying")]
    [SerializeField] private int selectedBuyAmount = 1;

    private readonly Dictionary<FoodConfig, int> foodAmounts = new Dictionary<FoodConfig, int>();
    private readonly List<FoodSlotUI> spawnedSlots = new List<FoodSlotUI>();

    public int SelectedBuyAmount => Mathf.Max(1, selectedBuyAmount);

    public event Action<int> SelectedBuyAmountChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (FoodConfig food in availableFoods)
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

    public void BuyFood(FoodConfig food)
    {
        BuyFood(food, SelectedBuyAmount);
    }

    public void BuyFood(FoodConfig food, int amount)
    {
        if (food == null)
            return;

        if (amount <= 0)
            return;

        if (GameManager.Instance == null)
            return;

        int totalPrice = food.price * amount;

        if (!GameManager.Instance.SpendCoins(totalPrice))
            return;

        AddFood(food, amount);
    }

    public void SetSelectedBuyAmount(int amount)
    {
        selectedBuyAmount = Mathf.Max(1, amount);
        RefreshInventoryUI();
        SelectedBuyAmountChanged?.Invoke(SelectedBuyAmount);
    }

    private void BuildInventoryUI()
    {
        ClearInventoryUI();

        foreach (FoodConfig food in availableFoods)
        {
            if (food == null)
                continue;

            FoodSlotUI slot = Instantiate(foodSlotPrefab, foodSlotParent);
            slot.Init(food);
            spawnedSlots.Add(slot);
        }

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        foreach (FoodSlotUI slot in spawnedSlots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    private void ClearInventoryUI()
    {
        foreach (FoodSlotUI slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }

    public List<FoodSaveData> GetSaveData()
    {
        List<FoodSaveData> data = new List<FoodSaveData>();

        foreach (FoodConfig food in availableFoods)
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

        foreach (FoodConfig food in availableFoods)
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
        foreach (FoodConfig food in availableFoods)
        {
            if (food != null && food.foodName == foodName)
                return food;
        }

        return null;
    }
}
