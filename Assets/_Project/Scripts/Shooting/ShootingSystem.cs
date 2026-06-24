using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.Shooting
{
    public class ShootingSystem : MonoBehaviour, IShootingSystem
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GameObject bulletPrefab;

        private ISceneManager sceneManager;
        private IPlayerManager playerManager;
        private List<BulletInfo> activeBullets = new();
        private List<Bullet> bulletObjects = new();
        private DamageCalculator damageCalc;

        public bool IsAnyBulletFlying => activeBullets.Count > 0;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            playerManager = FindObjectOfType<PlayerManager>();
            damageCalc = new DamageCalculator(config);
        }

        public void FireBullet(int ownerID, CurveData curve)
        {
            if (curve == null || !curve.IsValid || curve.Points == null || curve.Points.Count == 0)
                return;

            var bullet = new BulletInfo
            {
                OwnerPlayerID = ownerID,
                Curve = curve,
                Speed = config.BulletSpeed,
                Progress = 0f,
                CurrentPosition = curve.Points[0].WorldPos,
                HasExploded = false,
                DamageRadius = playerManager.GetPlayer(ownerID)?.DamageRadius ?? config.BaseDamageRadius,
                Damage = config.BaseDamage
            };
            activeBullets.Add(bullet);

            if (bulletPrefab != null)
            {
                var obj = Instantiate(bulletPrefab, bullet.CurrentPosition, Quaternion.identity);
                var comp = obj.GetComponent<Bullet>();
                comp?.Initialize(bullet, this);
                bulletObjects.Add(comp);
            }

            GameEvent.OnBulletFired?.Invoke(bullet);
            Debug.Log($"[Shooting] 子弹发射: 玩家{ownerID}, 曲线={curve.RawExpression}");
        }

        private void Update()
        {
            for (int i = activeBullets.Count - 1; i >= 0; i--)
            {
                UpdateBullet(activeBullets[i], Time.deltaTime);
            }
        }

        private void UpdateBullet(BulletInfo bullet, float dt)
        {
            if (bullet.HasExploded)
            {
                RemoveBullet(bullet);
                return;
            }

            float moveDist = bullet.Speed * dt;
            float newProgress = MoveAlongCurve(bullet, moveDist);
            bullet.Progress = newProgress;

            if (bullet.Progress >= 1f)
            {
                RemoveBullet(bullet);
                return;
            }

            CheckCollisions(bullet);
        }

        private float MoveAlongCurve(BulletInfo bullet, float distance)
        {
            var points = bullet.Curve.Points;
            if (points.Count < 2) return 1f;

            int startIdx = Mathf.FloorToInt(bullet.Progress * (points.Count - 1));
            startIdx = Mathf.Clamp(startIdx, 0, points.Count - 2);
            float remaining = distance;

            for (int i = startIdx; i < points.Count - 1; i++)
            {
                float segLen = Vector2.Distance(points[i].WorldPos, points[i + 1].WorldPos);
                if (segLen < 0.001f) continue;

                if (remaining <= segLen)
                {
                    float t = remaining / segLen;
                    bullet.CurrentPosition = Vector2.Lerp(points[i].WorldPos, points[i + 1].WorldPos, t);
                    return (i + t) / (points.Count - 1);
                }
                remaining -= segLen;
            }

            bullet.CurrentPosition = points[points.Count - 1].WorldPos;
            return 1f;
        }

        private void CheckCollisions(BulletInfo bullet)
        {
            if (!sceneManager.IsInsideMap(bullet.CurrentPosition))
            {
                RemoveBullet(bullet);
                return;
            }

            foreach (var obs in sceneManager.Obstacles)
            {
                if (IsHittingObstacle(bullet, obs))
                {
                    Explode(bullet, obs.Position);
                    return;
                }
            }

            foreach (var player in playerManager.GetAllPlayers())
            {
                if (player.PlayerID == bullet.OwnerPlayerID) continue;
                if (!player.IsAlive) continue;

                if (Vector2.Distance(bullet.CurrentPosition, player.Position) < config.PlayerRadius + 5f)
                {
                    Explode(bullet, player.Position);
                    return;
                }
            }
        }

        private bool IsHittingObstacle(BulletInfo bullet, ObstacleData obs)
        {
            float bulletR = 5f;
            if (obs.Shape == ObstacleShape.Circle)
            {
                return Vector2.Distance(bullet.CurrentPosition, obs.Position) < obs.Radius + bulletR;
            }
            else
            {
                return CollisionDetector.PointInRect(
                    bullet.CurrentPosition, obs.Position, obs.Size, obs.Rotation);
            }
        }

        private void Explode(BulletInfo bullet, Vector2 center)
        {
            bullet.HasExploded = true;
            GameEvent.OnBulletHitObstacle?.Invoke(bullet);
            damageCalc.ApplyExplosionDamage(bullet, center, sceneManager, playerManager);
        }

        private void RemoveBullet(BulletInfo bullet)
        {
            int idx = activeBullets.IndexOf(bullet);
            if (idx >= 0)
            {
                activeBullets.RemoveAt(idx);
                if (idx < bulletObjects.Count && bulletObjects[idx] != null)
                {
                    Destroy(bulletObjects[idx].gameObject);
                    bulletObjects.RemoveAt(idx);
                }
            }
        }

        public void ClearAllBullets()
        {
            foreach (var b in bulletObjects)
                if (b != null) Destroy(b.gameObject);
            activeBullets.Clear();
            bulletObjects.Clear();
        }

        public List<BulletInfo> GetActiveBullets() => activeBullets;

        public List<Vector2> GetBulletPreview(CurveData curve)
        {
            var preview = new List<Vector2>();
            if (curve?.Points != null)
                foreach (var p in curve.Points)
                    preview.Add(p.WorldPos);
            return preview;
        }
    }
}
