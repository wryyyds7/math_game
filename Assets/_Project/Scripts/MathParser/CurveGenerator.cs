using System;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Utils;

namespace MathGame.MathParser
{
    public class CurveGenerator
    {
        private GameConfig config;

        public CurveGenerator(GameConfig config)
        {
            this.config = config;
        }

        public List<CurvePoint> GenerateLocalPoints(Func<float, float> func, float maxX)
        {
            var points = new List<CurvePoint>();
            float step = config.CurveSampleStep;

            // 起点 x=0
            float y0 = SafeEval(func, 0);
            points.Add(new CurvePoint { LocalX = 0, LocalY = y0 });

            for (float x = step; x <= maxX; x += step)
            {
                float y = SafeEval(func, x);
                points.Add(new CurvePoint { LocalX = x, LocalY = y });
            }

            return points;
        }

        public List<CurvePoint> ConvertToWorld(List<CurvePoint> localPoints,
                                                Vector2 origin, float angleDeg)
        {
            foreach (var p in localPoints)
            {
                p.WorldPos = MathUtils.LocalToWorld(
                    new Vector2(p.LocalX, p.LocalY), origin, angleDeg);
            }
            return localPoints;
        }

        private float SafeEval(Func<float, float> func, float x)
        {
            try { return func(x); }
            catch { return 0; }
        }
    }
}
