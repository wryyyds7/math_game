using UnityEngine;

namespace MathGame.Core.Data
{
    public static class LevelConfig
    {
        private const float BaseExp = 100f;
        private const float Power = 1.5f;
        private const int MaxLevel = 20;

        public static int GetExpForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.RoundToInt(BaseExp * Mathf.Pow(level - 1, Power));
        }

        public static int GetExpToNextLevel(int currentLevel)
        {
            return GetExpForLevel(currentLevel + 1) - GetExpForLevel(currentLevel);
        }

        public static (bool leveledUp, int newLevel, int remainingExp)
            AddExp(int currentLevel, int currentExp, int expToAdd)
        {
            if (currentLevel >= MaxLevel) return (false, MaxLevel, currentExp);

            int newExp = currentExp + expToAdd;
            int newLevel = currentLevel;

            while (newLevel < MaxLevel && newExp >= GetExpForLevel(newLevel + 1))
            {
                newLevel++;
            }

            return (newLevel > currentLevel, newLevel, newExp);
        }
    }
}
