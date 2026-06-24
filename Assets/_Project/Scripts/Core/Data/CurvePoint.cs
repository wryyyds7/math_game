using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public struct CurvePoint
    {
        public float LocalX;
        public float LocalY;
        public Vector2 WorldPos;
    }
}
