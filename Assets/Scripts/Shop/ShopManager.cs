using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop")]
    public Transform contentParent;
    public ShopItemUI shopItemPrefab;

    private readonly List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    public void BuildShop()
    {
        ClearShop();

        foreach (DinoConfig config in GameManager.Instance.dinoConfigs)
        {
            ShopItemUI item = Instantiate(shopItemPrefab, contentParent);
            item.Init(config);
            spawnedItems.Add(item);
        }

        RefreshShop();
    }

    public void RefreshShop()
    {
        foreach (ShopItemUI item in spawnedItems)
        {
            item.Refresh();
        }
    }

    private void ClearShop()
    {
        foreach (ShopItemUI item in spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        spawnedItems.Clear();
    }
}