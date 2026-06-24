using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.SceneManagement
{
    public class SceneManager : MonoBehaviour, ISceneManager
    {
        [SerializeField] private GameConfig config;

        private ObstacleGenerator obstacleGen;
        private StarGenerator starGen;
        private List<ObstacleData> obstacles = new();
        private List<StarData> stars = new();
        private int nextObstacleID = 0;
        private int nextStarID = 0;

        public float MapWidth => config.MapWidth;
        public float MapHeight => config.MapHeight;
        public List<ObstacleData> Obstacles => obstacles;
        public List<StarData> Stars => stars;

        private void Awake()
        {
            obstacleGen = new ObstacleGenerator(config);
            starGen = new StarGenerator(config);
        }

        public void GenerateMap()
        {
            obstacles.Clear();
            stars.Clear();
            nextObstacleID = 0;
            nextStarID = 0;

            obstacles = obstacleGen.Generate(config.ObstacleCount, nextObstacleID);
            nextObstacleID = obstacles.Count;
            stars = starGen.Generate(config.StarCount, obstacles, nextStarID);
            nextStarID = stars.Count;

            Debug.Log($"[SceneManager] 地图生成: {obstacles.Count} 障碍物, {stars.Count} 星星");
        }

        public Vector2 GetValidSpawnPoint()
        {
            int maxAttempts = 100;
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(config.PlayerSpawnMargin, config.MapWidth - config.PlayerSpawnMargin),
                    Random.Range(config.PlayerSpawnMargin, config.MapHeight - config.PlayerSpawnMargin));

                if (!IsCollidingWithObstacle(pos, config.PlayerRadius))
                    return pos;
            }
            return new Vector2(config.MapWidth / 2f, config.MapHeight / 2f);
        }

        public bool IsInsideMap(Vector2 position)
        {
            return position.x >= 0 && position.x <= config.MapWidth
                && position.y >= 0 && position.y <= config.MapHeight;
        }

        public bool IsCollidingWithObstacle(Vector2 position, float radius)
        {
            foreach (var obs in obstacles)
            {
                float dist = Vector2.Distance(position, obs.Position);
                if (dist < obs.Radius + radius)
                    return true;
            }
            return false;
        }

        public StarData CollectStar(int starID)
        {
            var star = stars.Find(s => s.StarID == starID && !s.IsCollected);
            if (star != null)
            {
                star.IsCollected = true;
                GameEvent.OnStarCollected?.Invoke(-1, star);
            }
            return star;
        }

        public void DamageObstacle(int obstacleID, float damage)
        {
            var obs = obstacles.Find(o => o.ObstacleID == obstacleID);
            if (obs == null || !obs.IsDestructible) return;

            obs.Health -= damage;
            if (obs.Health <= 0)
                obstacles.Remove(obs);
        }
    }
}
