using System;
using System.Collections.Generic;

namespace MathGame.Core.Data
{
    [Serializable]
    public class CurveData
    {
        public string RawExpression;
        public List<CurvePoint> Points;
        public float TotalLength;
        public bool IsValid;
        public string ErrorMessage;
    }
}
