using System.Collections.Generic;
using System.Text.RegularExpressions;
using MathGame.Core.Data;

namespace MathGame.MathParser
{
    public class FunctionValidator
    {
        public bool ValidateSyntax(string expression, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(expression))
            {
                error = "表达式不能为空";
                return false;
            }

            string trimmed = expression.Trim();
            if (!trimmed.StartsWith("y="))
            {
                error = "表达式必须以 'y=' 开头";
                return false;
            }

            string body = trimmed.Substring(2).Trim();
            if (string.IsNullOrEmpty(body))
            {
                error = "表达式内容不能为空";
                return false;
            }

            // 安全字符检查
            if (!Regex.IsMatch(trimmed, @"^[y]=[a-zA-Z0-9\+\-\*/\\^%\(\)\.\,\s\piPIeE\=]+$"))
            {
                // 更宽松的检查
                if (Regex.IsMatch(trimmed, @"[;{}\[\]""'\\]"))
                {
                    error = "表达式包含不支持的字符";
                    return false;
                }
            }

            if (!AreBracketsBalanced(body))
            {
                error = "括号不匹配";
                return false;
            }

            return true;
        }

        public bool ValidateInMap(List<CurvePoint> points, float mapW, float mapH)
        {
            foreach (var p in points)
            {
                if (p.WorldPos.x < 0 || p.WorldPos.x > mapW ||
                    p.WorldPos.y < 0 || p.WorldPos.y > mapH)
                    return false;
            }
            return true;
        }

        private bool AreBracketsBalanced(string expr)
        {
            int depth = 0;
            foreach (char c in expr)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
                if (depth < 0) return false;
            }
            return depth == 0;
        }
    }
}
