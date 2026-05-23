using UnityEngine;
using TMPro;

public class RaidCoinsUI : MonoBehaviour
{
    public TMP_Text coinsText;

    private void Update()
    {
        if (coinsText == null)
            return;

        if (GameManager.Instance == null)
            return;

        coinsText.text = "Coins: " + GameManager.Instance.coins;
    }
}