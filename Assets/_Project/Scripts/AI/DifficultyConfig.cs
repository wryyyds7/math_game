using UnityEngine;

namespace MathGame.AI
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "MathGame/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("简单")]
        public float EasyThinkDelay = 1.0f;
        [Range(0, 90)] public float EasyAccuracyNoise = 30f;
        [Range(0, 1)] public float EasyShootChance = 0.5f;
        [Range(0, 1)] public float EasyBlinkChance = 0.2f;
        [Range(0, 1)] public float EasyRotateChance = 0.3f;

        [Header("普通")]
        public float NormalThinkDelay = 0.8f;
        [Range(0, 90)] public float NormalAccuracyNoise = 10f;
        [Range(0, 1)] public float NormalShootChance = 0.7f;
        [Range(0, 1)] public float NormalBlinkChance = 0.1f;
        [Range(0, 1)] public float NormalRotateChance = 0.1f;

        [Header("困难")]
        public float HardThinkDelay = 0.5f;
        [Range(0, 90)] public float HardAccuracyNoise = 3f;
        [Range(0, 1)] public float HardShootChance = 0.85f;
        [Range(0, 1)] public float HardBlinkChance = 0.1f;
        [Range(0, 1)] public float HardRotateChance = 0.05f;
    }
}
