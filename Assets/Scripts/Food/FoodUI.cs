using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text amountText;
    public DraggableFoodUI draggableFood;

    private FoodConfig foodConfig;

    public void Init(FoodConfig config)
    {
        foodConfig = config;

        if (iconImage != null)
            iconImage.sprite = foodConfig.icon;

        if (draggableFood != null)
            draggableFood.Init(foodConfig);

        Refresh();
    }

    public void Refresh()
    {
        if (foodConfig == null)
            return;

        int amount = FoodInventory.Instance != null
            ? FoodInventory.Instance.GetFoodAmount(foodConfig)
            : 0;

        bool hasFood = amount > 0;

        if (amountText != null)
            amountText.text = CoinFormatter.FormatNumber(amount);

        if (iconImage != null)
        {
            iconImage.sprite = foodConfig.icon;
            iconImage.color = hasFood ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }

        if (draggableFood != null)
            draggableFood.SetAvailable(hasFood);
    }
}
