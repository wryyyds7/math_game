using UnityEngine;

namespace MathGame.Core.Utils
{
    public static class RandomUtils
    {
        public static float Range(float min, float max)
        {
            return Random.Range(min, max);
        }

        public static int Range(int min, int maxExclusive)
        {
            return Random.Range(min, maxExclusive);
        }

        public static Vector2 RandomPointInCircle(Vector2 center, float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(0f, radius);
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        public static Vector2 RandomPointInRect(float xMin, float xMax, float yMin, float yMax)
        {
            return new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
        }

        public static float RandomAngle()
        {
            return Random.Range(0f, 360f);
        }

        public static T WeightedRandom<T>(T[] items, float[] weights)
        {
            float total = 0f;
            foreach (float w in weights) total += w;
            float rand = Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < items.Length; i++)
            {
                cumulative += weights[i];
                if (rand <= cumulative) return items[i];
            }
            return items[items.Length - 1];
        }
    }
}
