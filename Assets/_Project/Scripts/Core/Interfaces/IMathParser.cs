using System.Collections.Generic;
using UnityEngine;
using MathGame.Core.Data;

namespace MathGame.Core.Interfaces
{
    public interface IMathParser
    {
        CurveData ParseAndGenerate(string expression, Vector2 origin, float forwardAngle, float maxX);
        bool ValidateExpression(string expression, out string errorMessage);
        float CalculateCurveLength(List<CurvePoint> points);
        string[] GetSupportedFunctions();
    }
}
