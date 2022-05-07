using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSystem.Scripts.Expression
{

    public sealed class Expresssion
    {
        static InfixToRPN infixToRpn = new InfixToRPN();
        
        static IValue ParseExpression0(ParamStackEnv env,string sourceExpression, string[] tokens)
        {
            Stack<IValue> tokenValue = new Stack<IValue>();
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (infixToRpn.isOperator(token))
                {
                    if (tokenValue.Count == 0)
                    {
                        Debug.LogError("token empty,"+sourceExpression);
                    }
                    var b = tokenValue.Pop();
                    if (tokenValue.Count == 0)
                    {
                        Debug.LogError("token empty,"+sourceExpression);
                    }
                    var a = tokenValue.Pop();
                    var ret = OperatorValue(token,a, b);
                    tokenValue.Push(ret);
                }
                else
                {
                    var find = env.FindValue(token);
                    if (find != null)
                    {
                        tokenValue.Push(new Number(find.Value));
                    }
                    else
                    {
                        if (Double.TryParse(token, out var val))
                        {
                            tokenValue.Push(new Number(val));
                        }
                        else
                        {
                            throw new NotImplementedException(token);
                        }
                    }
                }
            }

            return tokenValue.Pop();
        }

        private static IValue OperatorValue(string operatorChar,IValue a, IValue b)
        {
            switch (operatorChar)
            {
                case "+":
                    return new OperationSum(new []{a,b});
                case "-":
                    return new OperationSum(new []{a,new OperationNegate(b)});
                case "*":
                case "∗":
                    return new OperationMul(new []{a,b});
                case "/":
                    return new OperationMul(new []{a,new OperationReciprocal(b)});
                case "^":
                    return new OperationPower(a,b);
                case "<":
                    return new BooleanNumber(a.Value<b.Value);
                case ">":
                    return new BooleanNumber(a.Value>b.Value);
                case "==":
                case "=":
                    return new BooleanNumber(Math.Abs(a.Value - b.Value) < Mathf.Epsilon);
                case "<=":
                    return new BooleanNumber(a.Value<=b.Value);
                case ">=":
                    return new BooleanNumber(a.Value>=b.Value);
                default:
                    throw new NotImplementedException(""+operatorChar);
                    break;
            }   
        }

        public static Dictionary<string,string[]> convertDict = new Dictionary<string, string[]>();
        public static IValue ParseExpression(ParamStackEnv env,string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }

            if (!convertDict.TryGetValue(expression, out var tokenList))
            {
                string newExpression = "";
                string tmpName = "";
                for (int i = 0; i < expression.Length; i++)
                {
                    var t = expression[i];
                    if (infixToRpn.isOperator(t+"")
                        || t=='(' || t==')')
                    {
                        newExpression += tmpName + " ";
                        tmpName = "";
                        newExpression += t + " ";
                    }
                    else
                    {
                        tmpName += t;
                    }
                }

                newExpression += tmpName + " ";

                var postfixExpression = infixToRpn.GetRPNExpression(newExpression);
                tokenList = postfixExpression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                convertDict.Add(expression,tokenList);
            }
            
            return ParseExpression0(env,expression, tokenList);
        }
    }
    public class ParamStackEnv
    {
        Dictionary<string,IValue> paramerrics = new Dictionary<string, IValue>();
        List<string> paramerKeys = new List<string>(4);
        private IValue defaultParamValue;
        public IValue FindValue(string c)
        {
            if (paramerrics.TryGetValue(c, out var ret))
            {
                return ret;
            }
            return null;
        }

        public IValue FirstValue
        {
            get
            {
                if (paramerKeys.Count > 0)
                {
                    return paramerrics[paramerKeys[0]];
                }
                else
                {
                    return defaultParamValue;
                }
            }
        }

        public void SetParamerrics(string paramerric, IValue parseExpression)
        {
            if (string.IsNullOrEmpty(paramerric))
            {
                defaultParamValue = parseExpression;
            }
            else
            {
                paramerrics[paramerric] = parseExpression;
                this.paramerKeys.Add(paramerric);
            }
        }

        public ParamStackEnv Clone()
        {
            ParamStackEnv paramStackEnv = new ParamStackEnv();
            foreach (var paramerric in paramerrics)
            {
                paramStackEnv.paramerrics.Add(paramerric.Key, paramerric.Value);
            }
            return paramStackEnv;
        }
    }
    
    public interface IValue
    {
        double Value { get; }
    }
    
    public class Number : IValue
    {
        private double m_Value;
        public double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
        public Number(double aValue)
        {
            m_Value = aValue;
        }
        public override string ToString()
        {
            return "" + m_Value + "";
        }
    }
    public class OperationSum : IValue
    {
        private IValue[] m_Values;
        public double Value
        {
            get { return m_Values.Select(v => v.Value).Sum(); }
        }
        public OperationSum(params IValue[] aValues)
        {
            // collapse unnecessary nested sum operations.
            List<IValue> v = new List<IValue>(aValues.Length);
            foreach (var I in aValues)
            {
                var sum = I as OperationSum;
                if (sum == null)
                    v.Add(I);
                else
                    v.AddRange(sum.m_Values);
            }
            m_Values = v.ToArray();
        }
        public override string ToString()
        {
            return "( " + string.Join(" + ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
        }
    }

    public class OperationMul : IValue
    {
        private IValue[] m_Values;
        public double Value
        {
            get { return m_Values.Select(v => v.Value).Aggregate((v1, v2) => v1 * v2); }
        }
        public OperationMul(params IValue[] aValues)
        {
            m_Values = aValues;
        }
        public override string ToString()
        {
            return "( " + string.Join(" * ", m_Values.Select(v => v.ToString()).ToArray()) + " )";
        }
    }
    
    public class OperationReciprocal : IValue
    {
        private IValue m_Value;
        public double Value
        {
            get { return 1.0 / m_Value.Value; }
        }
        public OperationReciprocal(IValue aValue)
        {
            m_Value = aValue;
        }
        public override string ToString()
        {
            return "( 1/" + m_Value + " )";
        }
    }
    
    public class OperationNegate : IValue
    {
        private IValue m_Value;
        public double Value
        {
            get { return -m_Value.Value; }
        }
        public OperationNegate(IValue aValue)
        {
            m_Value = aValue;
        }
        public override string ToString()
        {
            return "( -" + m_Value + " )";
        }

    }
    
    public class OperationPower : IValue
    {
        private IValue m_Value;
        private IValue m_Power;
        public double Value
        {
            get { return System.Math.Pow(m_Value.Value, m_Power.Value); }
        }
        public OperationPower(IValue aValue, IValue aPower)
        {
            m_Value = aValue;
            m_Power = aPower;
        }
        public override string ToString()
        {
            return "( " + m_Value + "^" + m_Power + " )";
        }

    }
    
    public class BooleanNumber : IValue
    {
        private double m_Value;
        public double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
        public BooleanNumber(bool aValue)
        {
            m_Value = aValue ? 1 : 0;
        }
        public override string ToString()
        {
            return "" + m_Value + "";
        }
    }
    
}