using UnityEngine;

[CreateAssetMenu(fileName = "DinoConfig", menuName = "Merge Dino/Dino Config")]
public class DinoConfig : ScriptableObject
{
    [Header("Main")]
    public int level = 1;
    public string dinoName = "Dino";

    [Header("Stages")]
    public Sprite[] stageSprites;

    [Header("Shop")]
    public int buyPrice = 50;

    [Header("Growth")]
    public float growthExperienceToNextStage = 100f;
    public float experiencePerSecondWhenFed = 4f;

    [Header("Food")]
    public float startSatiety = 0f;
    public float satietyLossPerSecond = 1f;

    [Header("Coins")]
    public float coinsPerSecond = 1f;

    [Header("Movement")]
    public float moveSpeed = 1.2f;
}