using System;
using System.Collections.Generic;
using System.Linq;

namespace LSystem.Scripts.Expression
{

    public sealed class Expresssion
    {
        public static IValue ParseExpression(ParamStackEnv env,string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }
            // int index = expression.IndexOf('(');
            //
            // while (index>0)
            // {
            //     var endIndex = expression.LastIndexOf(')');
            // }
            if (expression.Contains("+"))
            {
                var splitExp = expression.Split(new[] {'+'}, StringSplitOptions.RemoveEmptyEntries);
                List<IValue> splitValue = new List<IValue>(splitExp.Length);
                for (int i = 0; i < splitExp.Length; i++)
                {
                    var iv = ParseExpression(env,splitExp[i]);
                    splitValue.Add(iv);
                }
                return new OperationSum(splitValue.ToArray());
            }else if (expression.Contains("-"))
            {
                var splitExp = expression.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                List<IValue> splitValue = new List<IValue>(splitExp.Length);
                for (int i = 0; i < splitExp.Length; i++)
                {
                    var iv = ParseExpression(env,splitExp[i]);
                    if (i > 0)
                    {
                        iv = new OperationNegate(iv);
                    }
                    splitValue.Add(iv);
                }
                return new OperationSum(splitValue.ToArray());
            }
            else if (expression.Contains("*"))
            {
                var splitExp = expression.Split(new[] {'*'}, StringSplitOptions.RemoveEmptyEntries);
                List<IValue> splitValue = new List<IValue>(splitExp.Length);
                for (int i = 0; i < splitExp.Length; i++)
                {
                    var iv = ParseExpression(env,splitExp[i]);
                    splitValue.Add(iv);
                }
                return new OperationMul(splitValue.ToArray());
            }
            else if (expression.Contains("∗"))
            {
                var splitExp = expression.Split(new[] {'∗'}, StringSplitOptions.RemoveEmptyEntries);
                List<IValue> splitValue = new List<IValue>(splitExp.Length);
                for (int i = 0; i < splitExp.Length; i++)
                {
                    var iv = ParseExpression(env,splitExp[i]);
                    splitValue.Add(iv);
                }
                return new OperationMul(splitValue.ToArray());
            }
            else if (expression.Contains("/"))
            {
                var splitExp = expression.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                List<IValue> splitValue = new List<IValue>(splitExp.Length);
                for (int i = 0; i < splitExp.Length; i++)
                {
                    var iv = ParseExpression(env,splitExp[i]);
                    if (i > 0)
                    {
                        iv = new OperationReciprocal(iv);
                    }
                    splitValue.Add(iv);
                }
                return new OperationMul(splitValue.ToArray());
            }
            
            else if (expression.Contains("^"))
            {
                var splitExp = expression.Split(new[] {'^'}, StringSplitOptions.RemoveEmptyEntries);
                var baseValue = ParseExpression(env,splitExp[0]);
                var pow = ParseExpression(env,splitExp[1]);
                return new OperationPower(baseValue,pow);
            }
            

            var find = env.FindValue(expression);
            if (find != null)
            {
                return new Number(find.Value);
            }

            if (Double.TryParse(expression, out var val))
            {
                return new Number(val);
            }

            throw new NotImplementedException(expression);
        }
    }
    public class ParamStackEnv
    {
        Dictionary<string,IValue> paramerrics = new Dictionary<string, IValue>();
        public List<string> paramerKeys = new List<string>(4);

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
                return paramerrics[paramerKeys[0]];
            }
        }

        public void SetParamerrics(string paramerric, IValue parseExpression)
        {
            paramerrics[paramerric] = parseExpression;
            this.paramerKeys.Add(paramerric);
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
}