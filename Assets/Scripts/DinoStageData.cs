using UnityEngine;

[System.Serializable]
public class DinoStageData
{
    [Header("Stage")]
    public string stageName = "egg";
    public Sprite sprite;

    [Header("Growth")]
    [Tooltip("Tiks to next stage. 1 tick = 1 second. If 0 or less, the dino will instantly grow to the next stage when it reaches this stage.")]
    public float ticksToNextStage = 10f;

    [Header("Calories")]
    [Tooltip("How many calories this stage consumes per tick/second.")]
    public float caloriesConsumePerTick = 1f;

    [Header("Coins")]
    [Tooltip("How many coins this stage gives per tick/second. 0.5 = 1 coin per 2 seconds.")]
    public float coinsPerTick = 0.1f;

    [Header("Movement")]
    public bool canMove = true;
}