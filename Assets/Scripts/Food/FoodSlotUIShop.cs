using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodSlotUIShop : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text amountText;
    public TMP_Text priceText;
    public Button buyButton;

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

        Refresh();
    }

    public void Refresh()
    {
        if (foodConfig == null)
            return;

        bool unlocked = FoodShop.Instance != null &&
                        FoodShop.Instance.IsFoodUnlocked(foodConfig);

        if (amountText != null)
            amountText.text = "";

        int buyAmount = FoodShop.Instance != null
            ? FoodShop.Instance.SelectedBuyAmount
            : 1;

        if (priceText != null)
            priceText.text = unlocked ? CoinFormatter.FormatNumber(foodConfig.price * buyAmount) : "";

        if (iconImage != null)
        {
            iconImage.sprite = foodConfig.icon;
            iconImage.color = unlocked
                ? Color.white
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
            lockedText.gameObject.SetActive(false);
            lockedText.text = "";
        }

        RefreshBuyButton(buyButton, unlocked, buyAmount);
    }

    private void RefreshBuyButton(Button button, bool unlocked, int amount)
    {
        if (button == null)
            return;

        button.interactable = unlocked && CanBuyAmount(amount);
    }

    private bool CanBuyAmount(int amount)
    {
        if (foodConfig == null)
            return false;

        if (GameManager.Instance == null)
            return false;

        return GameManager.Instance.coins >= foodConfig.price * amount;
    }

    private void BuyFood()
    {
        if (foodConfig == null)
            return;

        if (FoodShop.Instance == null)
            return;

        FoodShop.Instance.BuyFood(foodConfig);
    }
}
