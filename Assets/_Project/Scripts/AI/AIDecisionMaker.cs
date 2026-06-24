using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.AI
{
    /// <summary>
    /// AI决策器 — 按难度区分行为策略
    /// </summary>
    public class AIDecisionMaker
    {
        private readonly Difficulty difficulty;
        private readonly DifficultyConfig config;

        public AIDecisionMaker(Difficulty diff, DifficultyConfig cfg)
        {
            this.difficulty = diff;
            this.config = cfg;
        }

        public (TurnActionType action, object data) DecideAction(
            PlayerState ai, List<PlayerState> allPlayers,
            List<ObstacleData> obstacles, CurveStrategy strategy,
            float mapWidth, float mapHeight)
        {
            return difficulty switch
            {
                Difficulty.Easy => DecideEasy(ai, allPlayers, obstacles, strategy),
                Difficulty.Normal => DecideNormal(ai, allPlayers, obstacles, strategy, mapWidth, mapHeight),
                Difficulty.Hard => DecideHard(ai, allPlayers, obstacles, strategy, mapWidth, mapHeight),
                _ => DecideEasy(ai, allPlayers, obstacles, strategy)
            };
        }

        /// <summary>简单难度：随机动作</summary>
        private (TurnActionType, object) DecideEasy(PlayerState ai, List<PlayerState> allPlayers,
                                                     List<ObstacleData> obstacles, CurveStrategy strategy)
        {
            float rand = Random.value;

            // 50%射击 (随机简单直线)
            if (rand < config.EasyShootChance)
            {
                float slope = Random.Range(0.3f, 2.5f);
                float offset = Random.Range(-60f, 60f);
                string expr = offset >= 0
                    ? $"y={slope:F1}*x+{offset:F0}"
                    : $"y={slope:F1}*x{offset:F0}";
                return (TurnActionType.Shoot, expr);
            }
            // 30%旋转
            if (rand < config.EasyShootChance + config.EasyRotateChance)
            {
                return (TurnActionType.Rotate180, null);
            }
            // 20%闪现
            if (ai.BlinkCharges > 0)
            {
                float range = ai.BlinkRange(1920, 1080);
                float angle = Random.Range(0f, Mathf.PI * 2);
                float dist = Random.Range(range * 0.3f, range);
                Vector2 target = ai.Position + new Vector2(
                    Mathf.Cos(angle) * dist,
                    Mathf.Sin(angle) * dist);
                target = ClampToMap(target, 1920, 1080);
                return (TurnActionType.Blink, target);
            }
            // 退化为射击
            return (TurnActionType.Shoot, $"y={Random.Range(0.5f, 2f):F1}*x");
        }

        /// <summary>普通难度：瞄准最近敌人</summary>
        private (TurnActionType, object) DecideNormal(PlayerState ai, List<PlayerState> allPlayers,
                                                       List<ObstacleData> obstacles, CurveStrategy strategy,
                                                       float mapWidth, float mapHeight)
        {
            PlayerState nearest = FindNearestEnemy(ai, allPlayers);
            if (nearest == null) return (TurnActionType.Shoot, "y=x");

            // 80%射击
            if (Random.value < config.NormalShootChance)
            {
                string expr = strategy.GenerateSimpleCurve(ai, nearest, obstacles);
                return (TurnActionType.Shoot, expr);
            }

            // 闪现
            if (ai.BlinkCharges > 0 && Random.value < config.NormalBlinkChance)
            {
                Vector2 dir = (nearest.Position - ai.Position).normalized;
                float range = ai.BlinkRange(mapWidth, mapHeight);
                Vector2 target = Random.value > 0.5f
                    ? ai.Position + dir * range * 0.7f   // 靠近
                    : ai.Position - dir * range * 0.7f;  // 远离
                return (TurnActionType.Blink, ClampToMap(target, mapWidth, mapHeight));
            }

            return (TurnActionType.Rotate180, null);
        }

        /// <summary>困难难度：精确最优决策</summary>
        private (TurnActionType, object) DecideHard(PlayerState ai, List<PlayerState> allPlayers,
                                                     List<ObstacleData> obstacles, CurveStrategy strategy,
                                                     float mapWidth, float mapHeight)
        {
            PlayerState bestTarget = FindBestTarget(ai, allPlayers, obstacles);
            if (bestTarget == null) return (TurnActionType.Shoot, "y=x");

            float bestAngle = strategy.CalculateOptimalAngle(ai, bestTarget, obstacles);
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(ai.Rotation, bestAngle));

            // 朝向已经合适，射击
            if (angleDiff < 30f)
            {
                string expr = strategy.GenerateOptimalCurve(ai, bestTarget, obstacles);
                return (TurnActionType.Shoot, expr);
            }

            // 旋转180度能对准
            float afterRotate = (ai.Rotation + 180f) % 360f;
            if (Mathf.Abs(Mathf.DeltaAngle(afterRotate, bestAngle)) < 45f)
            {
                return (TurnActionType.Rotate180, null);
            }

            // 闪现到有利位置
            if (ai.BlinkCharges > 0)
            {
                Vector2 blinkTarget = strategy.CalculateBestBlinkPosition(
                    ai, bestTarget, obstacles, mapWidth, mapHeight);
                return (TurnActionType.Blink, blinkTarget);
            }

            return (TurnActionType.Rotate180, null);
        }

        private PlayerState FindNearestEnemy(PlayerState self, List<PlayerState> all)
        {
            PlayerState nearest = null;
            float minDist = float.MaxValue;
            foreach (var p in all)
            {
                if (p.PlayerID == self.PlayerID || !p.IsAlive) continue;
                float dist = Vector2.Distance(self.Position, p.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = p;
                }
            }
            return nearest;
        }

        private PlayerState FindBestTarget(PlayerState self, List<PlayerState> all,
                                           List<ObstacleData> obstacles)
        {
            PlayerState best = null;
            float bestScore = float.MinValue;

            foreach (var p in all)
            {
                if (p.PlayerID == self.PlayerID || !p.IsAlive) continue;

                float dist = Vector2.Distance(self.Position, p.Position);
                // 评分：距离近 + 等级低 优先级高
                float score = -dist * 0.1f - p.Level * 5f;

                // 无障碍物挡路加分
                bool blocked = IsBlockedByObstacle(self.Position, p.Position, obstacles);
                if (!blocked) score += 50f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = p;
                }
            }
            return best ?? FindNearestEnemy(self, all);
        }

        private bool IsBlockedByObstacle(Vector2 from, Vector2 to, List<ObstacleData> obstacles)
        {
            Vector2 dir = (to - from).normalized;
            float dist = Vector2.Distance(from, to);

            foreach (var obs in obstacles)
            {
                // 点到线段距离
                Vector2 toObs = obs.Position - from;
                float proj = Vector2.Dot(toObs, dir);
                if (proj < 0 || proj > dist) continue;

                Vector2 closest = from + dir * proj;
                float perpDist = Vector2.Distance(obs.Position, closest);
                if (perpDist < obs.Radius + 10f) return true;
            }
            return false;
        }

        private Vector2 ClampToMap(Vector2 pos, float width, float height)
        {
            return new Vector2(
                Mathf.Clamp(pos.x, 20f, width - 20f),
                Mathf.Clamp(pos.y, 20f, height - 20f));
        }
    }
}
