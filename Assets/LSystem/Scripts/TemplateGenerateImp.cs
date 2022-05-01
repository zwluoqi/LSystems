using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LSystem.Scripts
{
    
    public class TemplateGenerateImp:IGenerateImp
    {
        List<char> param = new List<char>();
        Dictionary<char,Action<int>> templateDefine = new Dictionary<char, Action<int>>();
        Dictionary<char,Action<int>> totalDefine = new Dictionary<char, Action<int>>();

        
        public override void Generate(ShapeSetting shapeSetting)
        {
            if (string.IsNullOrEmpty(shapeSetting.templateRule))
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
            param.Clear();
            
            for (int i = 0; i < shapeSetting.templateRule.Length; i++)
            {
                if (shapeSetting.templateRule[i] > 'a' && shapeSetting.templateRule[i] < 'z'
                ||shapeSetting.templateRule[i] > 'A' && shapeSetting.templateRule[i] < 'Z')
                {
                    if (param.IndexOf(shapeSetting.templateRule[i]) < 0)
                    {
                        param.Add(shapeSetting.templateRule[i]);
                    }
                }
            }

            //define rule
            var rules = shapeSetting.templateRule.Split(new []{','},StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rules.Length; i++)
            {
                var define = rules[i].Substring(rules[i].IndexOf('=') + 1);
                templateDefine[rules[i][0]] = AnalyticDefine(shapeSetting,define);
                totalDefine[rules[i][0]] = templateDefine[rules[i][0]];
            }

            AddDefaultRule(shapeSetting);
            
            //init rule
            var initRule = AnalyticDefine(shapeSetting,shapeSetting.initRule);
            
            //calculate
            initRule(0);
            
            return;
        }

        private void AddDefaultRule(ShapeSetting shapeSetting)
        {
            void ActionF(int iter)
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
                        defineF(iter);
                    }
                }
                else
                {
                    AddCell(shapeSetting);
                    UpdatePos(shapeSetting);
                }
            }

            totalDefine['F'] = ActionF;

            void Actionf(int iter)
            {
                UpdatePos(shapeSetting);
            }

            totalDefine['f'] = Actionf; 
        }

        // F：前进，且建立几何体
        // f：前进，但不建立几何体
        // A, B, X, Y, Z（大写字体）：容器变量，用于存储生成规则和迭代，没有几何意义
        // +，-，&，^: 分别是往右，左，前，后旋转。如+F就是往右前进一个单位
        // ~：随机角度。如~F就是往随机角度前进一个单位
        // [ ]：分支结构
        
        private Action<int> AnalyticDefine(ShapeSetting shapeSetting,string define)
        {
            Action<int> tmp = delegate(int iter)
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
                                totalDefine[key](iter + 1);
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