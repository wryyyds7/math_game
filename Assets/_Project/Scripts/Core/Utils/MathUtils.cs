using UnityEngine;

namespace MathGame.Core.Utils
{
    public static class MathUtils
    {
        public static Vector2 AngleToDirection(float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        public static float DirectionToAngle(Vector2 dir)
        {
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static Vector2 RotateVector(Vector2 v, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public static Vector2 LocalToWorld(Vector2 localPos, Vector2 origin, float angleDeg)
        {
            Vector2 forward = AngleToDirection(angleDeg);
            Vector2 side = new Vector2(-forward.y, forward.x);
            return origin + localPos.x * forward + localPos.y * side;
        }

        public static float ClampAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }
    }
}
