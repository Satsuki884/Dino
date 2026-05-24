using UnityEngine;

[CreateAssetMenu(fileName = "DinoConfig", menuName = "Merge Dino/Dino Config")]
public class DinoConfig : ScriptableObject
{
    [Header("Main")]
    public int level = 1;
    public string dinoName = "Dino";

    [Header("Shop")]
    public int buyPrice = 10;

    [Header("Start")]
    public float startCalories = 0f;

    [Header("Movement")]
    public float moveSpeed = 1.2f;

    [Header("Balance")]
    public float growthTimeMultiplier = 2f;

    [Header("Raid")]
    public float coinPerRaid = 1f;
    public float timeToRaid = 10f;

    [Header("Stages")]
    public DinoStageData[] stages;
}