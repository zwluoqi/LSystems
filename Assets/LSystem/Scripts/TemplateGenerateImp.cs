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

        
        public override List<Vector3> Generate(ShapeSetting shapeSetting)
        {
            if (string.IsNullOrEmpty(shapeSetting.templateRule))
            {
                Debug.LogError("shapeSetting.templateRule is nil");
                return new List<Vector3>();
            }
            if (string.IsNullOrEmpty(shapeSetting.initRule))
            {
                Debug.LogError("shapeSetting.initRule is nil");
                return new List<Vector3>();
            }
            
            List<Vector3> vector3s = new List<Vector3>();

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
            Action<int> F = null;
            F = delegate(int iter)
            {
                if (templateDefine.TryGetValue('F', out var defineF))
                {
                    if (iter > shapeSetting.maxIter)
                    {
                        UpdateRect(shapeSetting.size);
                        AddCell(ref vector3s);
                        UpdatePos(shapeSetting.size);
                    }
                    else
                    {
                        defineF(iter);
                    }
                }
                else
                {
                    UpdateRect(shapeSetting.size);
                    AddCell(ref vector3s);
                    UpdatePos(shapeSetting.size);
                }
            };
            totalDefine['F'] = F; 
            
            //init rule
            var initRule = AnalyticDefine(shapeSetting,shapeSetting.initRule);
            
            //calculate
            initRule(0);
            
            return vector3s;
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
                            Rotation(shapeSetting.angle);
                            stringBuilder.AppendLine("Rotation(shapeSetting.angle)");
                            break;
                        case '-':
                            Rotation(-shapeSetting.angle);
                            stringBuilder.AppendLine("Rotation(-shapeSetting.angle)");
                            break;
                        default:
                            if (totalDefine.ContainsKey(key))
                            {
                                totalDefine[key](iter + 1);
                                stringBuilder.AppendLine(key + "(iter+1)");
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