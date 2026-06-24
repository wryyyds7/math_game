using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MathGame.MathParser
{
    /// <summary>
    /// 递归下降表达式解析器
    /// 语法: expression -> term (('+'|'-') term)*
    ///       term       -> factor (('*'|'/'|'%') factor)*
    ///       factor     -> power ('^' power)*
    ///       power      -> ('+'|'-')? atom
    ///       atom       -> NUMBER | 'x' | 'pi' | 'e' | FUNC '(' expression ')' | '(' expression ')'
    ///       FUNC       -> 'sin'|'cos'|'tan'|'asin'|'acos'|'atan'|'abs'|'sqrt'|'ln'|'log'|'exp'|'floor'|'ceil'
    /// </summary>
    public class FunctionEvaluator
    {
        private string input;
        private int pos;
        private float currentX;

        private static readonly HashSet<string> Functions = new()
        {
            "sin", "cos", "tan", "asin", "acos", "atan",
            "abs", "sqrt", "ln", "log", "log10", "exp", "floor", "ceil"
        };

        public Func<float, float> Parse(string expression)
        {
            // 去除 y= 前缀
            string body = expression.Trim();
            if (body.StartsWith("y="))
                body = body.Substring(2).Trim();

            // 预处理：处理隐式乘法 (2x -> 2*x)
            body = PreProcess(body);

            // 返回一个求值委托
            string expr = body;
            return (float x) => Evaluate(expr, x);
        }

        private string PreProcess(string expr)
        {
            // x 前面如果是数字或 )，插入 *
            string result = "";
            for (int i = 0; i < expr.Length; i++)
            {
                if (expr[i] == 'x' && i > 0)
                {
                    char prev = expr[i - 1];
                    if (char.IsDigit(prev) || prev == ')' || prev == 'e')
                        result += "*x";
                    else
                        result += 'x';
                }
                else
                {
                    result += expr[i];
                }
            }
            return result;
        }

        public float Evaluate(string expression, float xValue)
        {
            input = expression;
            pos = 0;
            currentX = xValue;
            float result = ParseExpression();
            if (pos < input.Length)
                Debug.LogWarning($"表达式解析未完成，剩余: {input.Substring(pos)}");
            return result;
        }

        private float ParseExpression()
        {
            float left = ParseTerm();
            while (pos < input.Length)
            {
                if (input[pos] == '+') { pos++; left += ParseTerm(); }
                else if (input[pos] == '-') { pos++; left -= ParseTerm(); }
                else break;
            }
            return left;
        }

        private float ParseTerm()
        {
            float left = ParseFactor();
            while (pos < input.Length)
            {
                char op = input[pos];
                if (op == '*') { pos++; left *= ParseFactor(); }
                else if (op == '/') { pos++; left /= ParseFactor(); }
                else if (op == '%') { pos++; left %= ParseFactor(); }
                else break;
            }
            return left;
        }

        private float ParseFactor()
        {
            float left = ParsePower();
            while (pos < input.Length && input[pos] == '^')
            {
                pos++;
                left = Mathf.Pow(left, ParsePower());
            }
            return left;
        }

        private float ParsePower()
        {
            // 一元正负号
            if (pos < input.Length && input[pos] == '+') { pos++; return ParseAtom(); }
            if (pos < input.Length && input[pos] == '-') { pos++; return -ParseAtom(); }
            return ParseAtom();
        }

        private float ParseAtom()
        {
            if (pos >= input.Length) return 0;

            // 数字
            if (char.IsDigit(input[pos]) || input[pos] == '.')
            {
                int start = pos;
                while (pos < input.Length && (char.IsDigit(input[pos]) || input[pos] == '.'))
                    pos++;
                float.TryParse(input.Substring(start, pos - start),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float val);
                return val;
            }

            // 变量 x
            if (input[pos] == 'x') { pos++; return currentX; }

            // 常量 pi
            if (MatchString("pi")) { return Mathf.PI; }

            // 常量 e
            if (pos < input.Length && input[pos] == 'e' &&
                (pos + 1 >= input.Length || !char.IsLetter(input[pos + 1]) || input[pos + 1] == 'x' || input[pos + 1] == '+' || input[pos + 1] == '-' || input[pos + 1] == '*' || input[pos + 1] == '/' || input[pos + 1] == '^' || input[pos + 1] == '%' || input[pos + 1] == ')'))
            {
                pos++;
                return Mathf.Exp(1f);
            }

            // 函数
            foreach (var func in Functions)
            {
                if (MatchString(func))
                {
                    SkipWhitespace();
                    if (pos < input.Length && input[pos] == '(')
                    {
                        pos++;
                        float arg = ParseExpression();
                        if (pos < input.Length && input[pos] == ')') pos++;
                        return ApplyFunction(func, arg);
                    }
                }
            }

            // 括号
            if (input[pos] == '(')
            {
                pos++;
                float val = ParseExpression();
                if (pos < input.Length && input[pos] == ')') pos++;
                return val;
            }

            return 0;
        }

        private void SkipWhitespace()
        {
            while (pos < input.Length && char.IsWhiteSpace(input[pos]))
                pos++;
        }

        private bool MatchString(string s)
        {
            SkipWhitespace();
            if (pos + s.Length > input.Length) return false;
            for (int i = 0; i < s.Length; i++)
                if (char.ToLowerInvariant(input[pos + i]) != char.ToLowerInvariant(s[i]))
                    return false;
            pos += s.Length;
            return true;
        }

        private float ApplyFunction(string func, float arg)
        {
            return func.ToLowerInvariant() switch
            {
                "sin"   => Mathf.Sin(arg),
                "cos"   => Mathf.Cos(arg),
                "tan"   => Mathf.Tan(arg),
                "asin"  => Mathf.Asin(Mathf.Clamp(arg, -1, 1)),
                "acos"  => Mathf.Acos(Mathf.Clamp(arg, -1, 1)),
                "atan"  => Mathf.Atan(arg),
                "abs"   => Mathf.Abs(arg),
                "sqrt"  => Mathf.Sqrt(Mathf.Max(0, arg)),
                "ln"    => Mathf.Log(Mathf.Max(0.0001f, arg)),
                "log"   => Mathf.Log10(Mathf.Max(0.0001f, arg)),
                "log10" => Mathf.Log10(Mathf.Max(0.0001f, arg)),
                "exp"   => Mathf.Exp(arg),
                "floor" => Mathf.Floor(arg),
                "ceil"  => Mathf.Ceil(arg),
                _ => arg
            };
        }
    }
}
