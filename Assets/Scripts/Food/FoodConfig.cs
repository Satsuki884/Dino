using UnityEngine;

[CreateAssetMenu(fileName = "FoodConfig", menuName = "Merge Dino/Food Config")]
public class FoodConfig : ScriptableObject
{
    [Header("Main")]
    public string foodName = "Meat";
    public Sprite icon;

    [Header("Shop")]
    public int price = 10;

    [Header("Effect")]
    public float satietyValue = 25f;
    public float bonusGrowthExperience = 0f;
    
    [Header("Unlock")]
    public int requiredDinoLevel = 1;
}