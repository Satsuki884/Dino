using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscoveryPopupUI : MonoBehaviour
{
    public static DiscoveryPopupUI Instance;

    [Header("Popup")]
    public GameObject root;
    public Image dinoIcon;
    public TMP_Text titleText;
    public TMP_Text levelText;
    public TMP_Text stageText;
    public TMP_Text coinsText;
    public Button closeButton;

    private void Awake()
    {
        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(DinoConfig config, int stage)
    {
        if (config == null)
            return;

        if (root != null)
            root.SetActive(true);

        if (dinoIcon != null && config.stageSprites != null && config.stageSprites.Length > 0)
        {
            int index = Mathf.Clamp(stage - 1, 0, config.stageSprites.Length - 1);
            dinoIcon.sprite = config.stageSprites[index];
        }

        if (titleText != null)
            titleText.text = "Новий динозаврик відкритий!";

        if (levelText != null)
            levelText.text = "Рівень: " + config.level;

        if (stageText != null)
            stageText.text = "Стадія: " + stage;

        if (coinsText != null)
            coinsText.text = "Монеток за секунду: " + config.coinsPerSecond;
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}