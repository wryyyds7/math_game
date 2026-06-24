using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public class BulletInfo
    {
        public int OwnerPlayerID;
        public CurveData Curve;
        public float Speed;
        public float Progress;
        public Vector2 CurrentPosition;
        public bool HasExploded;
        public float DamageRadius;
        public int Damage;
    }
}
