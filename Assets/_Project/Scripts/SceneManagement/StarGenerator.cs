using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.SceneManagement
{
    public class StarGenerator
    {
        private GameConfig config;

        // 权重：Small=60%, Medium=30%, Large=10%
        private static readonly (StarSize size, float weight)[] SizeWeights = new[]
        {
            (StarSize.Small, 60f),
            (StarSize.Medium, 30f),
            (StarSize.Large, 10f)
        };

        public StarGenerator(GameConfig config)
        {
            this.config = config;
        }

        public List<StarData> Generate(int count, List<ObstacleData> obstacles, int startID)
        {
            var stars = new List<StarData>();
            int idCounter = startID;
            int maxAttempts = 100;

            for (int i = 0; i < count; i++)
            {
                StarData star = null;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var candidate = CreateRandomStar(idCounter);
                    if (!IsInsideObstacle(candidate, obstacles))
                    {
                        star = candidate;
                        break;
                    }
                }
                if (star != null) { stars.Add(star); idCounter++; }
            }
            return stars;
        }

        private StarData CreateRandomStar(int id)
        {
            Vector2 pos = new Vector2(
                Random.Range(config.StarGenerateRadius, config.MapWidth - config.StarGenerateRadius),
                Random.Range(config.StarGenerateRadius, config.MapHeight - config.StarGenerateRadius));

            return new StarData
            {
                StarID = id,
                Size = GetWeightedRandomSize(),
                Position = pos,
                IsCollected = false
            };
        }

        private StarSize GetWeightedRandomSize()
        {
            float total = 0f;
            foreach (var (_, w) in SizeWeights) total += w;
            float rand = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var (size, w) in SizeWeights)
            {
                cumulative += w;
                if (rand <= cumulative) return size;
            }
            return StarSize.Small;
        }

        private bool IsInsideObstacle(StarData star, List<ObstacleData> obstacles)
        {
            foreach (var obs in obstacles)
            {
                float dist = Vector2.Distance(star.Position, obs.Position);
                if (dist < obs.Radius + 15f)
                    return true;
            }
            return false;
        }
    }
}
