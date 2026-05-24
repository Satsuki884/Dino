using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodBuyAmountSelectorUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button x1Button;
    public Button x10Button;
    public Button x100Button;

    [Header("Optional Text")]
    public TMP_Text selectedAmountText;

    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribeToInventory();
    }

    private void OnDisable()
    {
        if (FoodShop.Instance != null && isSubscribed)
            FoodShop.Instance.SelectedBuyAmountChanged -= RefreshSelectedAmount;

        isSubscribed = false;
    }

    private void Start()
    {
        TrySubscribeToInventory();

        SetupButton(x1Button);
        SetupButton(x10Button);
        SetupButton(x100Button);

        RefreshFromInventory();
    }

    private void TrySubscribeToInventory()
    {
        if (isSubscribed)
            return;

        if (FoodShop.Instance == null)
            return;

        FoodShop.Instance.SelectedBuyAmountChanged += RefreshSelectedAmount;
        isSubscribed = true;
    }

    private void SetupButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectNextAmount);
    }

    private void SelectNextAmount()
    {
        int currentAmount = FoodShop.Instance != null
            ? FoodShop.Instance.SelectedBuyAmount
            : 1;

        SelectAmount(GetNextAmount(currentAmount));
    }

    private int GetNextAmount(int currentAmount)
    {
        if (currentAmount == 1)
            return 10;

        if (currentAmount == 10)
            return 100;

        return 1;
    }

    private void SelectAmount(int amount)
    {
        if (FoodShop.Instance != null)
            FoodShop.Instance.SetSelectedBuyAmount(amount);

        RefreshSelectedAmount(amount);
    }

    private void RefreshFromInventory()
    {
        int amount = FoodShop.Instance != null
            ? FoodShop.Instance.SelectedBuyAmount
            : 1;

        RefreshSelectedAmount(amount);
    }

    private void RefreshSelectedAmount(int amount)
    {
        if (selectedAmountText != null)
            selectedAmountText.text = "x" + amount;

        RefreshButtonStates(amount);
    }

    private void RefreshButtonStates(int selectedAmount)
    {
        SetButtonVisible(x1Button, selectedAmount == 1);
        SetButtonVisible(x10Button, selectedAmount == 10);
        SetButtonVisible(x100Button, selectedAmount == 100);
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visible);
        button.interactable = visible;
    }

}
