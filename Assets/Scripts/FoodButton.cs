using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodButton : MonoBehaviour
{
    [Header("Food")]
    public FoodConfig foodConfig;

    [Header("UI")]
    public Button button;
    public TMP_Text priceText;

    private void Start()
    {
        if (priceText != null && foodConfig != null)
            priceText.text = foodConfig.foodName + ": " + foodConfig.price;

        if (button != null)
            button.onClick.AddListener(BuyFood);
    }

    private void BuyFood()
    {
        if (foodConfig == null)
        {
            Debug.LogWarning("FoodConfig is not assigned in FoodButton.");
            return;
        }

        if (FoodInventory.Instance == null)
        {
            Debug.LogWarning("FoodInventory not found on scene.");
            return;
        }

        FoodInventory.Instance.BuyFood(foodConfig);
    }
}