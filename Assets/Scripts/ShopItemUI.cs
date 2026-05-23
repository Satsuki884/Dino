using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public Image silhouetteImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button buyButton;

    private DinoConfig config;

    public void Init(DinoConfig newConfig)
    {
        config = newConfig;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(Buy);

        Refresh();
    }

    public void Refresh()
    {
        if (config == null)
            return;

        bool unlocked = GameManager.Instance.IsLevelUnlocked(config.level);
        bool hasSpace = GameManager.Instance.CanSpawnMoreDinos();

        if (nameText != null)
            nameText.text = config.dinoName + " Lv." + config.level;

        if (priceText != null)
            priceText.text = config.buyPrice.ToString();

        if (iconImage != null)
        {
            if (config.stageSprites != null && config.stageSprites.Length > 0)
                iconImage.sprite = config.stageSprites[0];

            iconImage.color = unlocked ? Color.white : Color.black;
        }

        if (silhouetteImage != null)
            silhouetteImage.gameObject.SetActive(!unlocked);

        if (buyButton != null)
            buyButton.interactable = unlocked && hasSpace;
    }

    private void Buy()
    {
        if (config == null)
            return;

        GameManager.Instance.BuyDino(config.level);
        Refresh();
    }
}