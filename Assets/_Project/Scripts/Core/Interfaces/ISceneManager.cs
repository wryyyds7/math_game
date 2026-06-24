using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface ISceneManager
    {
        float MapWidth { get; }
        float MapHeight { get; }
        List<ObstacleData> Obstacles { get; }
        List<StarData> Stars { get; }
        void GenerateMap();
        Vector2 GetValidSpawnPoint();
        bool IsInsideMap(Vector2 position);
        bool IsCollidingWithObstacle(Vector2 position, float radius);
        StarData CollectStar(int starID);
        void DamageObstacle(int obstacleID, float damage);
    }
}
