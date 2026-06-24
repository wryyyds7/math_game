using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        public int PlayerID { get; private set; }
        private PlayerState state;
        private IPlayerManager playerManager;
        private PlayerVisual visual;

        public void Initialize(int playerID, IPlayerManager pm)
        {
            PlayerID = playerID;
            playerManager = pm;
            state = pm.GetPlayer(playerID);
            visual = GetComponent<PlayerVisual>();
            visual?.Initialize(state);
        }

        private void Update()
        {
            if (state == null) return;

            // 平滑移动
            transform.position = Vector2.Lerp(transform.position, state.Position, Time.deltaTime * 10f);

            // 平滑旋转（精灵默认朝上，偏移-90°）
            float targetAngle = state.Rotation;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle - 90f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);

            visual?.Refresh();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 检测碰到星星
            if (other.CompareTag("Star"))
            {
                var starComponent = other.GetComponent<StarPickup>();
                if (starComponent != null && !starComponent.IsCollected)
                {
                    starComponent.Collect(PlayerID);
                }
            }
        }
    }

    /// <summary>
    /// 挂载在星星GameObject上的拾取组件
    /// </summary>
    public class StarPickup : MonoBehaviour
    {
        public int StarID;
        public StarSize Size;
        public bool IsCollected { get; private set; }

        private ISceneManager sceneManager;

        public void Init(int id, StarSize size, ISceneManager sm)
        {
            StarID = id;
            Size = size;
            sceneManager = sm;
            IsCollected = false;

            float scale = size switch
            {
                StarSize.Small => 0.5f,
                StarSize.Medium => 0.8f,
                StarSize.Large => 1.2f,
                _ => 0.5f
            };
            transform.localScale = Vector3.one * scale;
        }

        public void Collect(int playerID)
        {
            if (IsCollected) return;
            IsCollected = true;

            var star = sceneManager?.CollectStar(StarID);
            if (star != null)
            {
                GameEvent.OnStarCollected?.Invoke(playerID, star);
            }
            Destroy(gameObject);
        }
    }
}
