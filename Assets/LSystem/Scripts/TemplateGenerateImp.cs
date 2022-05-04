using System;
using System.Collections.Generic;
using System.Text;
using LSystem.Scripts.Expression;
using UnityEngine;

namespace LSystem.Scripts
{
    public class RuleDefine
    {
        /// <summary>
        /// 函数名
        /// </summary>
        public char key;
        /// <summary>
        /// 函数参数
        /// </summary>
        public string[] paramerrics = new string[4];
        /// <summary>
        /// 函数回调
        /// </summary>
        Action<int,ParamStackEnv> publicAction;

        public void CallAction(int iter,ParamStackEnv paramStackEnv,string paramExpression)
        {
            ParamStackEnv paramStackEnv0 = paramStackEnv.Clone();
            var expressions = paramExpression.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < expressions.Length; i++)
            {
                paramStackEnv0.SetParamerrics(paramerrics[i], Expresssion.ParseExpression(paramStackEnv,expressions[i]));
            }
            publicAction(iter, paramStackEnv0);
        }

        public RuleDefine(char key, string[] paramerrics,Action<int,ParamStackEnv> action)
        {
            this.key = key;
            this.publicAction = action;
            Array.Copy(paramerrics,this.paramerrics,this.paramerrics.Length);
        }
        public RuleDefine(char key,Action<int,ParamStackEnv> action)
        {
            this.key = key;
            this.publicAction = action;

        }
    }
    
    public class TemplateGenerateImp:IGenerateImp
    {
        Dictionary<char,RuleDefine> templateDefine = new Dictionary<char, RuleDefine>();
        Dictionary<char,RuleDefine> totalDefine = new Dictionary<char, RuleDefine>();

        
        public override void Generate(ShapeSetting shapeSetting)
        {
            if (string.IsNullOrEmpty(shapeSetting.templateRule)&& shapeSetting.templateRules.Length == 0)
            {
                Debug.LogError("shapeSetting.templateRule is nil");
                return;
            }
            if (string.IsNullOrEmpty(shapeSetting.initRule))
            {
                Debug.LogError("shapeSetting.initRule is nil");
                return;
            }
            

            templateDefine.Clear();
            totalDefine.Clear();

            ParamStackEnv paramStackEnv = new ParamStackEnv();
            //define rule
            if (!string.IsNullOrEmpty(shapeSetting.templateRule))
            {
                DefineTemplateRule(shapeSetting);
            }
            else
            {
                DefineArrayTemplateRule(shapeSetting);
            }

            //default rule
            AddDefaultRule(shapeSetting);

            //define const
            DefineConst(shapeSetting,ref paramStackEnv);
            
            //init rule
            var initRule = AnalyticDefine(shapeSetting,shapeSetting.initRule);
            
            //calculate
            initRule(0,paramStackEnv);
            
            return;
        }

        private void DefineConst(ShapeSetting shapeSetting,ref ParamStackEnv paramStackEnv)
        {
            //TODO 需要一个独立的词法分析单元
            foreach (var constantDefine in shapeSetting.constantDefines)
            {
                if (string.IsNullOrEmpty(constantDefine))
                {
                    continue;
                }
                var keyIndex = constantDefine.IndexOf('=');
                var key = constantDefine.Substring(0, keyIndex );
                var value = constantDefine.Substring(keyIndex + 1);
                var iv = Expresssion.ParseExpression(paramStackEnv,value);
                paramStackEnv.SetParamerrics(key,iv);
            }
        }

        private void DefineTemplateRule(ShapeSetting shapeSetting)
        {
            //define rule
            var rules = shapeSetting.templateRule.Split(new []{','},StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rules.Length; i++)
            {
                var defineAction = rules[i].Substring(rules[i].IndexOf('=') + 1);
                var action = AnalyticDefine(shapeSetting,defineAction);
                var ruleDefine = new RuleDefine(rules[i][0],action);
                
                templateDefine[ruleDefine.key] = ruleDefine;
                totalDefine[ruleDefine.key] = ruleDefine;
            }
        }
        
        private void DefineArrayTemplateRule(ShapeSetting shapeSetting)
        {
            //define rule
            // var rules = shapeSetting.templateRule.Split(new []{','},StringSplitOptions.RemoveEmptyEntries);
            foreach (var rule in shapeSetting.templateRules)
            {
                var splitIndex = rule.IndexOf('→');
                int size = 1;
                if (splitIndex < 0)
                {
                    splitIndex = rule.IndexOf("->", StringComparison.Ordinal);
                    size = 2;
                }
                var defineKey = rule.Substring(0, splitIndex);
                var defineAction = rule.Substring( splitIndex + size);
                
                string[] parametricNames = new string[4];
                string parameName = "";
                int parameNameNum = 0;
                bool startParametric = false;
                bool startParameName = false;
                foreach (var t in defineKey)
                {
                    if (t == '(')
                    {
                        startParametric = true;
                        continue;
                    }
                    if (t == ')')
                    {
                        if (startParameName)
                        {
                            parametricNames[parameNameNum++] = parameName;
                            parameName = "";
                        }
                        startParametric = false;
                        continue;
                    }

                    if (startParametric)
                    {
                        if (t > 'a' && t < 'z'
                            || t > 'A' && t < 'Z')
                        {
                            startParameName = true;
                        }
                        else
                        {
                            startParameName = false;
                            parametricNames[parameNameNum++] = parameName;
                            parameName = "";
                        }

                        if (startParameName)
                        {
                            parameName += t;
                        }

                    }
                }
                var action = AnalyticDefine(shapeSetting,defineAction);
                var ruleDefine = new RuleDefine(defineKey[0],parametricNames,action);

                templateDefine[ruleDefine.key] = ruleDefine;
                totalDefine[ruleDefine.key] = ruleDefine;
            }
        }

        private void AddDefaultRule(ShapeSetting shapeSetting)
        {
            void ActionF(int iter,ParamStackEnv paramStackEnv)
            {
                if (templateDefine.TryGetValue('F', out var defineF))
                {
                    if (iter > shapeSetting.maxIter)
                    {
                        float paramValue = 0.0f;
                        if (paramStackEnv.FirstValue != null)
                        {
                            paramValue = (float)paramStackEnv.FirstValue.Value;
                        }
                        
                        AddCell(shapeSetting,paramValue);
                        UpdatePos(shapeSetting,paramValue);
                    }
                    else
                    {
                        defineF.CallAction(iter,paramStackEnv,"");
                    }
                }
                else
                {
                    AddCell(shapeSetting);
                    UpdatePos(shapeSetting);
                }
            }

            if (templateDefine.TryGetValue('F', out var defineF))
            {
                totalDefine['F'] = new RuleDefine('F',defineF.paramerrics,ActionF);
            }
            else
            {                
                totalDefine['F'] = new RuleDefine('F',ActionF);
            }


            void Actionf(int iter,ParamStackEnv paramStackEnv)
            {
                float paramValue = 0.0f;
                if (paramStackEnv.FirstValue != null)
                {
                    paramValue = (float)paramStackEnv.FirstValue.Value;
                }
                UpdatePos(shapeSetting,paramValue);
            }

            if (templateDefine.TryGetValue('f', out var definef))
            {
                totalDefine['f'] = new RuleDefine('f',definef.paramerrics, Actionf);
            }
            else
            {
                totalDefine['f'] = new RuleDefine('f', Actionf);
            }
        }

        // F：前进，且建立几何体
        // f：前进，但不建立几何体
        // A, B, X, Y, Z（大写字体）：容器变量，用于存储生成规则和迭代，没有几何意义
        // +，-，&，^: 分别是往右，左，前，后旋转。如+F就是往右前进一个单位
        // ~：随机角度。如~F就是往随机角度前进一个单位
        // [ ]：分支结构
        
        private Action<int,ParamStackEnv> AnalyticDefine(ShapeSetting shapeSetting,string define)
        {
            // string.Format()
            Action<int,ParamStackEnv> tmp = delegate(int iter,ParamStackEnv paramStackEnv)
            {
                if (iter > shapeSetting.maxIter)
                {
                    return;
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < define.Length; i++)
                {
                    var key = define[i];
                    int newIndex = i;
                    var lambdaExpression = GetLambdaExpression(define,ref newIndex);
                    switch (key)
                    {
                        case '[':
                            PushEnv();
                            stringBuilder.AppendLine("PushEnv");
                            break;
                        case ']':
                            PopEvn();
                            stringBuilder.AppendLine("PopEvn");
                            break;
                        case '+':
                        {
                            var iv = Expresssion.ParseExpression(paramStackEnv, lambdaExpression);
                            if (iv == null)
                            {
                                Turn(shapeSetting.angle);
                            }
                            else
                            {
                                Turn((float)iv.Value); 
                            }
                        }
                            stringBuilder.AppendLine("Turn(+)");
                            break;
                        case '-':
                        case '−':
                            Turn(-shapeSetting.angle);
                            stringBuilder.AppendLine("Turn(-)");
                            break;
                        case '&':
                        {
                            var iv = Expresssion.ParseExpression(paramStackEnv, lambdaExpression);
                            if (iv == null)
                            {
                                Pitch(shapeSetting.angle);
                            }
                            else
                            {
                                Pitch((float)iv.Value); 
                            }
                        }
                            stringBuilder.AppendLine("Pitch(+)");
                            break;
                        case '^':
                        case '∧':
                            Pitch(-shapeSetting.angle);
                            stringBuilder.AppendLine("Pitch(-)");
                            break;
                        case '\\':
                        {
                            var iv = Expresssion.ParseExpression(paramStackEnv, lambdaExpression);
                            if (iv == null)
                            {
                                Roll(shapeSetting.angle);
                            }
                            else
                            {
                                Roll((float)iv.Value); 
                            }
                        }
                            stringBuilder.AppendLine("Roll(+)");
                            break;
                        case '/':
                            Roll(-shapeSetting.angle);
                            stringBuilder.AppendLine("Roll(-)");
                            break;
                        case '|':
                            TurnBack();
                            stringBuilder.AppendLine("TurnBack()");
                            break;
                        case '<':
                            DivideLength(shapeSetting);
                            stringBuilder.AppendLine("DivideLength(shapeSetting)");
                            break;
                        case '>':
                            MultipleLength(shapeSetting);
                            stringBuilder.AppendLine("MultipleLength(shapeSetting)");
                            break;
                        case '#':
                            IncrementWidth(shapeSetting);
                            stringBuilder.AppendLine("IncrementWidth(shapeSetting)");
                            break;
                        case '!':
                            DecrementWidth(shapeSetting);
                            stringBuilder.AppendLine("DecrementWidth(shapeSetting)");
                            break;
                        case '{':
                            StartSaveSubsequentPos();
                            break;
                        case '}':
                            FillSavedPolygon();
                            break;
                        case '\'':
                        case '’':
                            //TODO color
                            break;
                        default:
                            if (totalDefine.ContainsKey(key))
                            {
                                totalDefine[key].CallAction(iter + 1, paramStackEnv, lambdaExpression);
                                stringBuilder.AppendLine(key + "(iter+1)");
                                if (!string.IsNullOrEmpty( lambdaExpression))
                                {
                                    i = newIndex + 1;
                                }
                            }
                            else
                            {
                                Debug.LogError("not support "+key);
                            }

                            break;
                    }
                }
                // Debug.LogWarning(stringBuilder.ToString());
            };
            return tmp;
        }

        private string GetLambdaExpression(string define, ref int i)
        {
            if (i < define.Length-1)
            {
                if (define[i + 1] == '(')
                {
                    int curCheckIndex = i;
                    do
                    {
                        i++;
                    } while (define[i + 1] != ')');

                    var lambdaExpression =
                        define.Substring(curCheckIndex + 2, i - curCheckIndex - 1);
                    return lambdaExpression;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}