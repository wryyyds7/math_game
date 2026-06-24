using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.Shooting
{
    public class DamageCalculator
    {
        private GameConfig config;

        public DamageCalculator(GameConfig config)
        {
            this.config = config;
        }

        public void ApplyExplosionDamage(BulletInfo bullet, Vector2 center,
                                         ISceneManager scene, IPlayerManager players)
        {
            float radius = bullet.DamageRadius;

            // 伤害玩家
            foreach (var player in players.GetAllPlayers())
            {
                if (!player.IsAlive) continue;
                if (player.PlayerID == bullet.OwnerPlayerID) continue;

                if (Vector2.Distance(center, player.Position) <= radius)
                {
                    players.EliminatePlayer(player.PlayerID);
                    GameEvent.OnBulletHitPlayer?.Invoke(player.PlayerID, bullet.OwnerPlayerID);

                    // 击杀计数
                    var owner = players.GetPlayer(bullet.OwnerPlayerID);
                    if (owner != null) owner.Kills++;
                }
            }

            // 伤害障碍物
            foreach (var obs in scene.Obstacles)
            {
                if (!obs.IsDestructible) continue;
                float dist = Vector2.Distance(center, obs.Position);
                if (dist <= radius + obs.Radius)
                {
                    float damage = bullet.Damage * (1f - dist / (radius + obs.Radius));
                    scene.DamageObstacle(obs.ObstacleID, damage);
                }
            }

            GameEvent.OnShowMessage?.Invoke($"爆炸！伤害半径: {radius:F1}");
        }
    }
}
