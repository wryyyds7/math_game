using UnityEngine;

namespace MathGame.Shooting
{
    public static class CollisionDetector
    {
        public static bool PointInCircle(Vector2 point, Vector2 center, float radius)
        {
            return Vector2.Distance(point, center) <= radius;
        }

        public static bool CircleToCircle(Vector2 c1, float r1, Vector2 c2, float r2)
        {
            return Vector2.Distance(c1, c2) <= r1 + r2;
        }

        public static bool SegmentToCircle(Vector2 a, Vector2 b, Vector2 center, float radius)
        {
            Vector2 ab = b - a;
            Vector2 ac = center - a;
            float sqrLen = Vector2.Dot(ab, ab);
            float t = sqrLen > 0 ? Mathf.Clamp01(Vector2.Dot(ac, ab) / sqrLen) : 0;
            Vector2 closest = a + t * ab;
            return Vector2.Distance(closest, center) <= radius;
        }

        public static bool PointInRect(Vector2 point, Vector2 rectCenter, Vector2 size, float rotation = 0)
        {
            Vector2 local = point - rectCenter;
            if (rotation != 0)
            {
                float rad = -rotation * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
                local = new Vector2(local.x * cos - local.y * sin,
                                     local.x * sin + local.y * cos);
            }
            Vector2 half = size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y;
        }
    }
}
