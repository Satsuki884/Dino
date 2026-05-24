using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodShop : MonoBehaviour
{
    public static FoodShop Instance;

    [Header("Shop UI")]
    public Transform foodSlotParent;
    public FoodSlotUIShop foodSlotPrefab;

    [Header("Available Food")]
    public List<FoodConfig> availableFoods = new List<FoodConfig>();

    [Header("Buying")]
    [SerializeField] private int selectedBuyAmount = 1;

    private readonly List<FoodSlotUIShop> spawnedSlots = new List<FoodSlotUIShop>();

    public int SelectedBuyAmount => Mathf.Max(1, selectedBuyAmount);

    public event Action<int> SelectedBuyAmountChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildShopUI();
    }

    public bool IsFoodUnlocked(FoodConfig food)
    {
        return food != null;
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

        if (FoodInventory.Instance == null)
        {
            Debug.LogWarning("Cannot buy food because FoodInventory is missing on scene.");
            return;
        }

        int totalPrice = food.price * amount;

        if (!GameManager.Instance.SpendCoins(totalPrice))
            return;

        FoodInventory.Instance.AddFood(food, amount);
        RefreshShopUI();
    }

    public void SetSelectedBuyAmount(int amount)
    {
        selectedBuyAmount = Mathf.Max(1, amount);
        RefreshShopUI();
        SelectedBuyAmountChanged?.Invoke(SelectedBuyAmount);
    }

    private void BuildShopUI()
    {
        ClearShopUI();

        foreach (FoodConfig food in GetConfiguredFoods())
        {
            if (food == null)
                continue;

            FoodSlotUIShop slot = Instantiate(foodSlotPrefab, foodSlotParent);
            slot.Init(food);
            spawnedSlots.Add(slot);
        }

        RefreshShopUI();
    }

    public void RefreshShopUI()
    {
        foreach (FoodSlotUIShop slot in spawnedSlots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    private void ClearShopUI()
    {
        foreach (FoodSlotUIShop slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }

    private List<FoodConfig> GetConfiguredFoods()
    {
        if (availableFoods != null && availableFoods.Count > 0)
            return availableFoods;

        if (FoodInventory.Instance != null)
            return FoodInventory.Instance.availableFoods;

        return availableFoods;
    }
}
