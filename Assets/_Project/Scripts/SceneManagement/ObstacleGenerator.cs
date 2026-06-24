using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Utils;

namespace MathGame.SceneManagement
{
    public class ObstacleGenerator
    {
        private GameConfig config;

        public ObstacleGenerator(GameConfig config)
        {
            this.config = config;
        }

        public List<ObstacleData> Generate(int count, int startID)
        {
            var obstacles = new List<ObstacleData>();
            int idCounter = startID;
            int maxAttempts = 100;

            for (int i = 0; i < count; i++)
            {
                ObstacleData obs = null;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var candidate = CreateRandomObstacle(idCounter);
                    if (!IsOverlapping(candidate, obstacles))
                    {
                        obs = candidate;
                        break;
                    }
                }

                if (obs != null)
                {
                    obstacles.Add(obs);
                    idCounter++;
                }
            }

            return obstacles;
        }

        private ObstacleData CreateRandomObstacle(int id)
        {
            float margin = config.ObstacleMaxRadius + 20f;
            Vector2 pos = new Vector2(
                Random.Range(margin, config.MapWidth - margin),
                Random.Range(margin, config.MapHeight - margin));

            ObstacleShape shape = Random.value < 0.7f ? ObstacleShape.Circle : ObstacleShape.Rectangle;
            float radius = Random.Range(config.ObstacleMinRadius, config.ObstacleMaxRadius);

            return new ObstacleData
            {
                ObstacleID = id,
                Shape = shape,
                Position = pos,
                Radius = radius,
                Size = shape == ObstacleShape.Rectangle
                    ? new Vector2(radius * 2f, radius * Random.Range(1f, 2f))
                    : Vector2.zero,
                Rotation = shape == ObstacleShape.Rectangle ? Random.Range(0f, 360f) : 0f,
                Health = 100f,
                IsDestructible = true
            };
        }

        private bool IsOverlapping(ObstacleData newObs, List<ObstacleData> existing)
        {
            float minGap = 20f;
            foreach (var obs in existing)
            {
                float dist = Vector2.Distance(newObs.Position, obs.Position);
                if (dist < newObs.Radius + obs.Radius + minGap)
                    return true;
            }
            return false;
        }
    }
}
