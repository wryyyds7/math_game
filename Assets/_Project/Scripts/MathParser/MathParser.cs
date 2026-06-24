using System;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;
using MathGame.Core.Interfaces;

namespace MathGame.MathParser
{
    public class MathParser : MonoBehaviour, IMathParser
    {
        [SerializeField] private GameConfig config;

        private FunctionEvaluator evaluator;
        private CurveGenerator curveGen;
        private FunctionValidator validator;

        private void Awake()
        {
            evaluator = new FunctionEvaluator();
            curveGen = new CurveGenerator(config);
            validator = new FunctionValidator();
        }

        public CurveData ParseAndGenerate(string expression, Vector2 origin,
                                          float forwardAngle, float maxX)
        {
            var curve = new CurveData { RawExpression = expression };

            if (!ValidateExpression(expression, out string error))
            {
                curve.IsValid = false;
                curve.ErrorMessage = error;
                return curve;
            }

            // 解析表达式
            string body = expression.Trim();
            if (body.StartsWith("y=")) body = body.Substring(2).Trim();

            try
            {
                Func<float, float> func = evaluator.Parse(expression);
                var localPoints = curveGen.GenerateLocalPoints(func, maxX);
                curve.Points = curveGen.ConvertToWorld(localPoints, origin, forwardAngle);
                curve.TotalLength = CalculateCurveLength(curve.Points);
                curve.IsValid = validator.ValidateInMap(curve.Points, config.MapWidth, config.MapHeight);

                if (!curve.IsValid)
                    curve.ErrorMessage = "曲线超出地图边界";
            }
            catch (Exception e)
            {
                curve.IsValid = false;
                curve.ErrorMessage = $"计算错误: {e.Message}";
            }

            return curve;
        }

        public bool ValidateExpression(string expression, out string errorMessage)
        {
            return validator.ValidateSyntax(expression, out errorMessage);
        }

        public float CalculateCurveLength(List<CurvePoint> points)
        {
            float length = 0;
            for (int i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1].WorldPos, points[i].WorldPos);
            return length;
        }

        public string[] GetSupportedFunctions()
        {
            return new[] { "sin", "cos", "tan", "asin", "acos", "atan",
                          "abs", "sqrt", "ln", "log", "exp", "floor", "ceil",
                          "pi", "e", "+", "-", "*", "/", "^", "%", "()" };
        }
    }
}
