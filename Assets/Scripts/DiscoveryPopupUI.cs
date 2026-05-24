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
        {
            closeButton.onClick.AddListener(PlayClick);
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    public void Show(DinoConfig config, int stage)
    {
        if (config == null)
            return;

        if (root != null)
            root.SetActive(true);

        DinoStageData stageData = null;

        if (config.stages != null && config.stages.Length > 0)
        {
            int index = Mathf.Clamp(stage - 1, 0, config.stages.Length - 1);
            stageData = config.stages[index];
        }

        if (dinoIcon != null && stageData != null)
            dinoIcon.sprite = stageData.sprite;

        if (titleText != null)
            titleText.text = "New Dinosaur Discovered!";

        if (levelText != null)
            levelText.text = "Level: " + config.level;

        if (stageText != null)
        {
            string stageName = stageData != null ? stageData.stageName : stage.ToString();
            stageText.text = "Stage: " + stageName;
        }

        if (coinsText != null)
        {
            float coins = stageData != null ? stageData.coinsPerTick : 0f;
            coinsText.text = "Coins per Second: " + coins;
        }
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void PlayClick()
    {
        if (AudioManager.Instanse != null)
            AudioManager.Instanse.PlayClick();
    }
}
