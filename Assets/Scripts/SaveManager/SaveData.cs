using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int coins;
    public int highestUnlockedLevel;

    public long lastSaveUnixTime;

    public List<DinoSaveData> dinos = new List<DinoSaveData>();
    public List<FoodSaveData> foods = new List<FoodSaveData>();
    public List<DinoEggPriceSaveData> dinoEggPrices = new List<DinoEggPriceSaveData>();
}

[Serializable]
public class DinoSaveData
{
    public int level;
    public int stage;

    public float positionX;
    public float positionY;
    public float positionZ;

    public float calories;
    public float growthTicks;

    public bool isInRaid;

    public float raidTimeLeft;
    public float raidDuration;
    public float raidRewardCoins;

    public float raidReturnPositionX;
    public float raidReturnPositionY;
    public float raidReturnPositionZ;
}

[Serializable]
public class FoodSaveData
{
    public string foodName;
    public int amount;
}

[Serializable]
public class DinoEggPriceSaveData
{
    public int level;
    public int purchaseCount;
}
