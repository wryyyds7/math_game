using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public class PlayerState
    {
        public int PlayerID;
        public string PlayerName;
        public bool IsAlive = true;
        public bool IsAI;

        public Vector2 Position;
        public float Rotation;

        public Vector2 Forward => new Vector2(
            Mathf.Cos(Rotation * Mathf.Deg2Rad),
            Mathf.Sin(Rotation * Mathf.Deg2Rad)
        );

        public int Level = 1;
        public int CurrentExp = 0;
        public int ExpToNextLevel => LevelConfig.GetExpForLevel(Level + 1);

        public int BlinkCharges = 2;
        public int MaxBlinkCharges = 2;

        public bool HasActedThisTurn = false;

        public int Kills = 0;
        public int StarsCollected = 0;

        public float DamageRadius => 0.5f + Level * 0.15f;

        public float BlinkRange(float mapWidth, float mapHeight)
        {
            float diagonal = Mathf.Sqrt(mapWidth * mapWidth + mapHeight * mapHeight);
            return diagonal * 0.3f;
        }
    }
}
