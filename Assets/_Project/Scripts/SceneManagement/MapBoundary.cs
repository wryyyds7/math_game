using UnityEngine;

namespace MathGame.SceneManagement
{
    public class MapBoundary : MonoBehaviour
    {
        public float Width = 1920f;
        public float Height = 1080f;
        public Color BoundaryColor = Color.red;

        private void OnDrawGizmos()
        {
            Gizmos.color = BoundaryColor;
            Vector3 center = new Vector3(Width / 2f, Height / 2f, 0);
            Vector3 size = new Vector3(Width, Height, 0);
            Gizmos.DrawWireCube(center, size);
        }

        public Vector2 ClampToMap(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, 0, Width),
                Mathf.Clamp(position.y, 0, Height));
        }
    }
}
