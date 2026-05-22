using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text amountText;
    public TMP_Text priceText;
    public Button buyButton;
    public DraggableFoodUI draggableFood;

    private FoodConfig foodConfig;

    public void Init(FoodConfig config)
    {
        foodConfig = config;

        if (iconImage != null)
            iconImage.sprite = foodConfig.icon;

        if (priceText != null)
            priceText.text = foodConfig.price.ToString();

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyFood);
        }

        if (draggableFood != null)
            draggableFood.Init(foodConfig);

        Refresh();
    }

    public void Refresh()
    {
        if (foodConfig == null)
            return;

        int amount = FoodInventory.Instance.GetFoodAmount(foodConfig);

        if (amountText != null)
            amountText.text = amount.ToString();

        if (draggableFood != null)
            draggableFood.SetAvailable(amount > 0);
    }

    private void BuyFood()
    {
        FoodInventory.Instance.BuyFood(foodConfig);
    }
}