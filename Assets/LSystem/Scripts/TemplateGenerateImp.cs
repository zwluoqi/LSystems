using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LSystem.Scripts
{

    public class ParamStackEnv
    {
        public Dictionary<string,float> paramerrics = new Dictionary<string, float>();
    }
    public class RuleDefine
    {
        /// <summary>
        /// 函数名
        /// </summary>
        public char key;
        /// <summary>
        /// 函数参数
        /// </summary>
        public char[] paramerrics = new char[4];
        /// <summary>
        /// 参数个数
        /// </summary>
        int paramNum = 0;
        /// <summary>
        /// 函数回调
        /// </summary>
        Action<int,ParamStackEnv> publicAction;

        public void CallAction(int iter,ParamStackEnv paramStackEnv,string paramExpression)
        {
            var expressions = paramExpression.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            
            publicAction(iter, paramStackEnv);
        }

        public RuleDefine(char key, char[] paramerrics,Action<int,ParamStackEnv> action)
        {
            this.key = key;
            this.publicAction = action;
            Array.Copy(paramerrics,this.paramerrics,this.paramerrics.Length);
            foreach (var t in this.paramerrics)
            {
                if (t != 0)
                {
                    paramNum++;
                }
            }
        }
        public RuleDefine(char key,Action<int,ParamStackEnv> action)
        {
            this.key = key;
            this.publicAction = action;

        }
    }
    
    public class TemplateGenerateImp:IGenerateImp
    {
        // Dictionary<char,RuleDefine> ruleDefines = new Dictionary<char, RuleDefine>()
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
            // ruleDefines.Clear();

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
            DefineConst(shapeSetting);
            
            //init rule
            var initRule = AnalyticDefine(shapeSetting,shapeSetting.initRule);
            
            //calculate
            ParamStackEnv paramStackEnv = new ParamStackEnv();
            initRule(0,paramStackEnv);
            
            return;
        }

        private void DefineConst(ShapeSetting shapeSetting)
        {
            //需要一个独立的词法分析单元
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
                var splitIndex = rule.IndexOf("->", StringComparison.Ordinal);
                var defineKey = rule.Substring(0, splitIndex);
                var defineAction = rule.Substring( splitIndex+ 2);
                bool startParametric = false;
                char[] parametrics = new char[4];
                int i = 0;
                foreach (var t in defineKey)
                {
                    if (t == '(')
                    {
                        startParametric = true;
                    }
                    if (t == ')')
                    {
                        startParametric = false;
                    }

                    if (startParametric)
                    {
                        if (t > 'a' && t < 'z'
                            || t > 'A' && t < 'Z')
                        {
                            parametrics[i++] = t;
                        }
                    }
                }
                var action = AnalyticDefine(shapeSetting,defineAction);
                var ruleDefine = new RuleDefine(defineKey[0],parametrics,action);

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
                        AddCell(shapeSetting);
                        UpdatePos(shapeSetting);
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

            totalDefine['F'] = new RuleDefine('F',ActionF);

            void Actionf(int iter,ParamStackEnv paramStackEnv)
            {
                UpdatePos(shapeSetting);
            }

            totalDefine['f'] = new RuleDefine('f',Actionf);
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
                            Turn(shapeSetting.angle);
                            stringBuilder.AppendLine("Turn(+)");
                            break;
                        case '-':
                            Turn(-shapeSetting.angle);
                            stringBuilder.AppendLine("Turn(-)");
                            break;
                        case '&':
                            Pitch(shapeSetting.angle);
                            stringBuilder.AppendLine("Pitch(+)");
                            break;
                        case '^':
                        case '∧':
                            Pitch(-shapeSetting.angle);
                            stringBuilder.AppendLine("Pitch(-)");
                            break;
                        case '\\':
                            Roll(shapeSetting.angle);
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
                                            define.Substring(curCheckIndex + 2, i - curCheckIndex - 2);
                                        totalDefine[key].CallAction(iter + 1, paramStackEnv, lambdaExpression);
                                    }
                                    else
                                    {
                                        totalDefine[key].CallAction(iter + 1, paramStackEnv, "");
                                    }
                                }
                                else
                                {
                                    totalDefine[key].CallAction(iter + 1, paramStackEnv, "");
                                }

                                stringBuilder.AppendLine(key + "(iter+1)");
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


    }
}