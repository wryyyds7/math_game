using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.AI
{
    /// <summary>
    /// AI专用的曲线生成策略
    /// </summary>
    public class CurveStrategy
    {
        private readonly IMathParser mathParser;

        public CurveStrategy(IMathParser parser)
        {
            this.mathParser = parser;
        }

        /// <summary>生成简单直线（普通难度）</summary>
        public string GenerateSimpleCurve(PlayerState ai, PlayerState target,
                                          List<ObstacleData> obstacles)
        {
            Vector2 toTarget = target.Position - ai.Position;
            Vector2 forward = ai.Forward;
            Vector2 side = new Vector2(-forward.y, forward.x);

            float localX = Vector2.Dot(toTarget, forward);
            float localY = Vector2.Dot(toTarget, side);

            if (localX <= 0) return "y=0"; // 目标在背后

            float slope = localY / localX;
            // 添加小幅随机偏移
            float offset = Random.Range(-20f, 20f);
            return offset >= 0
                ? $"y={slope:F2}*x+{offset:F1}"
                : $"y={slope:F2}*x{offset:F1}";
        }

        /// <summary>计算最佳射击角度（困难难度），遍历所有角度评分</summary>
        public float CalculateOptimalAngle(PlayerState ai, PlayerState target,
                                           List<ObstacleData> obstacles)
        {
            float bestAngle = ai.Rotation;
            float bestScore = float.MinValue;

            for (float angle = 0; angle < 360; angle += 5f)
            {
                float score = ScoreAngle(ai, target, obstacles, angle);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAngle = angle;
                }
            }
            return bestAngle;
        }

        private float ScoreAngle(PlayerState ai, PlayerState target,
                                 List<ObstacleData> obstacles, float angle)
        {
            Vector2 forward = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector2 toTarget = target.Position - ai.Position;
            Vector2 side = new Vector2(-forward.y, forward.x);

            float localX = Vector2.Dot(toTarget, forward);
            float localY = Vector2.Dot(toTarget, side);

            if (localX <= 0) return -100f; // 目标在背后

            float score = 100f / (1f + Mathf.Abs(localY) * 0.01f); // 越靠近中心线分越高

            // 检查障碍物挡路
            foreach (var obs in obstacles)
            {
                Vector2 toObs = obs.Position - ai.Position;
                float obsLocalX = Vector2.Dot(toObs, forward);
                float obsLocalY = Vector2.Dot(toObs, side);

                if (obsLocalX > 0 && obsLocalX < localX
                    && Mathf.Abs(obsLocalY) < obs.Radius + 20f)
                {
                    score -= 30f; // 障碍物挡路扣分
                }
            }

            return score;
        }

        /// <summary>生成最优曲线（困难难度）</summary>
        public string GenerateOptimalCurve(PlayerState ai, PlayerState target,
                                           List<ObstacleData> obstacles)
        {
            Vector2 forward = ai.Forward;
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2 toTarget = target.Position - ai.Position;

            float localX = Vector2.Dot(toTarget, forward);
            float localY = Vector2.Dot(toTarget, side);

            if (localX <= 0) return "y=x";

            // 检查是否有障碍物在直线上
            bool hasObstacleInWay = false;
            foreach (var obs in obstacles)
            {
                Vector2 toObs = obs.Position - ai.Position;
                float obsLocalX = Vector2.Dot(toObs, forward);
                float obsLocalY = Vector2.Dot(toObs, side);

                if (obsLocalX > 0 && obsLocalX < localX
                    && Mathf.Abs(obsLocalY) < obs.Radius + 15f)
                {
                    hasObstacleInWay = true;
                    break;
                }
            }

            if (hasObstacleInWay)
            {
                // 用正弦波绕开障碍物
                float amp = localY > 0 ? 80f : -80f;
                return $"y={amp:F0}*sin(0.005*x+0)+0*x";
            }
            else
            {
                float slope = localY / localX;
                return $"y={slope:F2}*x";
            }
        }

        /// <summary>计算最佳闪现位置（困难难度）</summary>
        public Vector2 CalculateBestBlinkPosition(PlayerState ai, PlayerState target,
                                                   List<ObstacleData> obstacles,
                                                   float mapWidth, float mapHeight)
        {
            float blinkRange = ai.BlinkRange(mapWidth, mapHeight);
            Vector2 bestPos = ai.Position;
            float bestScore = float.MinValue;

            // 在闪现圆上搜索多个候选点
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector2 candidate = ai.Position + new Vector2(
                    Mathf.Cos(angle) * blinkRange * 0.8f,
                    Mathf.Sin(angle) * blinkRange * 0.8f);

                // 确保在地图内
                if (candidate.x < 0 || candidate.x > mapWidth ||
                    candidate.y < 0 || candidate.y > mapHeight)
                    continue;

                // 评分：靠近目标且远离障碍物
                float distToTarget = Vector2.Distance(candidate, target.Position);
                float score = -distToTarget;

                foreach (var obs in obstacles)
                {
                    float distToObs = Vector2.Distance(candidate, obs.Position);
                    if (distToObs < obs.Radius + 30f)
                        score -= 50f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPos = candidate;
                }
            }
            return bestPos;
        }
    }
}
