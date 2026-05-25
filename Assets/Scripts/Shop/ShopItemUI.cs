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

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (config == null)
            return;

        bool unlocked = GameManager.Instance.IsDinoEggUnlockedInShop(config.level);
        bool hasSpace = GameManager.Instance.CanSpawnMoreDinos();

        if (nameText != null)
            nameText.text = config.dinoName;

        if (priceText != null)
            priceText.text = CoinFormatter.FormatNumber(GameManager.Instance.GetDinoEggPrice(config.level));

        Sprite firstStageSprite = GetFirstStageSprite();

        if (iconImage != null)
        {
            iconImage.sprite = firstStageSprite;
            iconImage.color = unlocked ? Color.white : Color.black;
        }

        if (silhouetteImage != null)
        {
            silhouetteImage.sprite = firstStageSprite;
            silhouetteImage.gameObject.SetActive(!unlocked);
            silhouetteImage.color = Color.black;
        }

        if (buyButton != null)
            buyButton.interactable = unlocked && hasSpace;
    }

    private Sprite GetFirstStageSprite()
    {
        if (config == null)
            return null;

        if (config.stages == null)
            return null;

        if (config.stages.Length == 0)
            return null;

        if (config.stages[0] == null)
            return null;

        return config.stages[0].sprite;
    }

    private void Buy()
    {
        if (config == null)
            return;

        bool bought = GameManager.Instance.BuyDino(config.level);

        if (!bought && AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();

        Refresh();
    }
}
