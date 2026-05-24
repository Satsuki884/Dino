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

    [Header("Locked View")]
    public Image silhouetteImage;
    public TMP_Text lockedText;

    private FoodConfig foodConfig;

    public void Init(FoodConfig config)
    {
        foodConfig = config;

        if (iconImage != null)
            iconImage.sprite = foodConfig.icon;

        if (silhouetteImage != null)
        {
            silhouetteImage.sprite = foodConfig.icon;
            silhouetteImage.color = Color.black;
        }

        if (priceText != null)
            priceText.text = CoinFormatter.FormatNumber(foodConfig.price);

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

        bool unlocked = FoodInventory.Instance != null &&
                        FoodInventory.Instance.IsFoodUnlocked(foodConfig);

        int amount = FoodInventory.Instance != null
            ? FoodInventory.Instance.GetFoodAmount(foodConfig)
            : 0;

        bool hasFood = amount > 0;

        if (amountText != null)
            amountText.text = unlocked ? amount.ToString() : "";

        if (priceText != null)
            priceText.text = unlocked ? CoinFormatter.FormatNumber(foodConfig.price) : "";

        if (iconImage != null)
        {
            iconImage.sprite = foodConfig.icon;
            iconImage.color = unlocked
                ? (hasFood ? Color.white : new Color(1f, 1f, 1f, 0.35f))
                : Color.black;
        }

        if (silhouetteImage != null)
        {
            silhouetteImage.sprite = foodConfig.icon;
            silhouetteImage.gameObject.SetActive(!unlocked);
            silhouetteImage.color = Color.black;
        }

        if (lockedText != null)
        {
            lockedText.gameObject.SetActive(!unlocked);
            lockedText.text = "Lv." + foodConfig.requiredDinoLevel;
        }

        if (buyButton != null)
            buyButton.interactable = unlocked;

        if (draggableFood != null)
            draggableFood.SetAvailable(unlocked && hasFood);
    }

    private void BuyFood()
    {
        if (foodConfig == null)
            return;

        if (FoodInventory.Instance == null)
            return;

        FoodInventory.Instance.BuyFood(foodConfig);
    }
}
