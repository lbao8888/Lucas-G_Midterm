using UnityEngine;

[CreateAssetMenu(fileName = "RatQuestConfig", menuName = "Quests/Rat Quest Config")]
public class RatQuestConfig : ScriptableObject
{
    [Min(1)] public int targetRatCount = 12;
    [Min(0)] public int rewardCoins = 100;
    [Min(0.1f)] public float captureDistance = 2.25f;
    [Min(0.1f)] public float fleeDistance = 7f;
    [Min(0.1f)] public float wanderRadius = 10f;
    [Min(0.1f)] public float wanderSpeed = 2.2f;
    [Min(0.1f)] public float fleeSpeed = 4.5f;
}
