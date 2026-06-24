using System;
using UnityEngine;

namespace MathGame.Core.Data
{
    [Serializable]
    public struct Vector2Data
    {
        public float x;
        public float y;

        public Vector2Data(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2(Vector2Data d) => new Vector2(d.x, d.y);
        public static implicit operator Vector2Data(Vector2 v) => new Vector2Data(v.x, v.y);
    }
}
