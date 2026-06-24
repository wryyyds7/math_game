using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public class ObstacleData
    {
        public int ObstacleID;
        public ObstacleShape Shape;
        public Vector2 Position;
        public float Radius;
        public Vector2 Size;
        public float Rotation;
        public float Health;
        public bool IsDestructible;
    }
}
