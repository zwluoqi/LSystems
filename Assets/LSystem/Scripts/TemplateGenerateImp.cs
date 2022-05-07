using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSystem.Scripts.Expression;
using UnityEngine;

namespace LSystem.Scripts
{
    
    
    public class TemplateGenerateImp:IGenerateImp
    {
        Dictionary<char,RuleDefine> totalDefine = new Dictionary<char, RuleDefine>();
        Dictionary<char,PredefineShape> preDefineShapes = new Dictionary<char, PredefineShape>();
        
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
            

            preDefineShapes.Clear();
            foreach (var predefine in shapeSetting.predefineShapes)
            {
                preDefineShapes[predefine.shapeKey] = predefine;
            }
            
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
                if (!totalDefine.TryGetValue(rules[i][0], out var ruleDefine))
                {
                    ruleDefine = new RuleDefine(rules[i][0]);
                    totalDefine[ruleDefine.key] = ruleDefine;
                }
                ruleDefine.AddBranch(action);
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
                defineKey = defineKey.Replace( " ", "");
                var defineAction = rule.Substring( splitIndex + size);
                defineAction = defineAction.Replace( " ", "");
                
                
                ///参数
                string[] parametricNames = new string[4];
                var startParametricIndex = defineKey.IndexOf('(');
                var endParametricIndex = defineKey.IndexOf(')');
                if (startParametricIndex > 0 && endParametricIndex > 0)
                {
                    var parameNameStr = defineKey.Substring(startParametricIndex + 1,
                        endParametricIndex - startParametricIndex - 1);
                    var splitParams = parameNameStr.Split(new[] {','});
                    Array.Copy(splitParams, parametricNames, Mathf.Min(splitParams.Length, 4));
                }

                //条件
                var startConditionIdnex = defineKey.IndexOf(':');
                string conditionExpression = "";
                if (startConditionIdnex > 0 && !defineKey.Contains('*'))
                {
                    conditionExpression = defineKey.Substring(startConditionIdnex + 1);
                }

                var action = AnalyticDefine(shapeSetting,defineAction);

                if (!totalDefine.TryGetValue(defineKey[0], out var ruleDefine))
                {
                    ruleDefine = new RuleDefine(defineKey[0],parametricNames);
                    totalDefine[ruleDefine.key] = ruleDefine;
                }
                ruleDefine.AddBranch(conditionExpression, action);
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
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < define.Length; i++)
                {
                    var key = define[i];
                    int newIndex = i;
                    var parametricExpression = GetParametricExpression(define,ref newIndex);
                    if (iter > shapeSetting.maxIter)
                    {
                        KeywordProcess(paramStackEnv,stringBuilder,key,shapeSetting,parametricExpression);
                    }
                    else
                    {
                        if (totalDefine.ContainsKey(key))
                        {
                            var success = totalDefine[key].AttemptAction(iter + 1, paramStackEnv, parametricExpression);
                            if (success)
                            {
                                WriteFunctionCall(stringBuilder, key + "(iter+1)");
                            }
                            else
                            {
                                KeywordProcess(paramStackEnv,stringBuilder,key, shapeSetting, parametricExpression);
                            }
                        }
                        else
                        {
                            KeywordProcess(paramStackEnv,stringBuilder,key, shapeSetting, parametricExpression);
                        }
                    }

                    if (!string.IsNullOrEmpty( parametricExpression))
                    {
                        i = newIndex + 1;
                    }
                }
                // Debug.LogWarning(stringBuilder.ToString());
            };
            return tmp;
        }

        private void KeywordProcess(ParamStackEnv paramStackEnv,StringBuilder stringBuilder, char key,ShapeSetting shapeSetting,string parametricExpression)
        {
            float v = 0;
            IValue iv = null;
            switch (key)
            {
                case '[':
                    PushEnv();
                    WriteFunctionCall(stringBuilder,"PushEnv");
                    break;
                case ']':
                    PopEvn();
                    WriteFunctionCall(stringBuilder,"PopEvn");
                    break;
                case '+':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Turn(iv == null ? shapeSetting.angle : v);
                    WriteFunctionCall(stringBuilder,"Turn(+)");
                    break;
                case '-':
                case '−':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Turn(iv == null ? -shapeSetting.angle : -v);
                    WriteFunctionCall(stringBuilder,"Turn(-)");
                    break;
                case '&':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Pitch(iv == null ?shapeSetting.angle:v);
                    WriteFunctionCall(stringBuilder,"Pitch(+)");
                    break;
                case '^':
                case '∧':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Pitch(iv == null ? -shapeSetting.angle : -v);
                    WriteFunctionCall(stringBuilder,"Pitch(-)");
                    break;
                case '\\':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Roll(iv == null ?shapeSetting.angle:v);
                    WriteFunctionCall(stringBuilder,"Roll(+)");
                    break;
                case '/':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    Roll(iv == null ?-shapeSetting.angle:-v);
                    WriteFunctionCall(stringBuilder,"Roll(-)");
                    break;
                case '|':
                    TurnBack();
                    WriteFunctionCall(stringBuilder,"TurnBack()");
                    break;
                case '$':
                    RotateTurtleToVertical();
                    WriteFunctionCall(stringBuilder,"RotateTurtleToVertical()");
                    break;
                case '<':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    DivideLength(iv == null ?shapeSetting.lengthFactor:v);
                    WriteFunctionCall(stringBuilder,"DivideLength(shapeSetting)");
                    break;
                case '>':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    MultipleLength(iv == null ?shapeSetting.lengthFactor:v);
                    WriteFunctionCall(stringBuilder,"MultipleLength(shapeSetting)");
                    break;
                case '#':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    IncrementWidth(iv == null ?  shapeSetting.widthIncrementFactor : v);
                    WriteFunctionCall(stringBuilder,"IncrementWidth(shapeSetting)");
                    break;
                case '!':
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    DecrementWidth(iv == null ?  shapeSetting.widthIncrementFactor : v);
                    WriteFunctionCall(stringBuilder, "DecrementWidth(shapeSetting)");
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
                case 'G'://前进
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    UpdatePos(shapeSetting,v);
                    break;
                case '.'://记录节点
                    SavePos(shapeSetting);
                    break;
                case  'f'://记录节点前进
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    SavePos(shapeSetting);
                    UpdatePos(shapeSetting,v);
                    break;
                case 'F'://绘制图形前进
                    iv = Expresssion.ParseExpression(paramStackEnv, parametricExpression);
                    if (iv != null)
                    {
                        v = (float)iv.Value;
                    }
                    AddCell(shapeSetting,v);
                    UpdatePos(shapeSetting,v);
                    break;
                default:
                    if (preDefineShapes.TryGetValue(key,out var predefineShape))
                    {
                        AddPredefineShape(predefineShape);
                    }else if (!totalDefine.ContainsKey(key))
                    {
                        Debug.LogError("not support " + key);
                    }
                    break;
            }
        }

        private void AddPredefineShape(PredefineShape predefineShape)
        {
            var predefineShapeData = new PredefineMeshData
                {pos = curEvn.pos, up = curEvn.up, right = curEvn.right, shareMesh = predefineShape.shape};
            generateMeshData.subPredefineDatas.Add(predefineShapeData);
        }


        private void WriteFunctionCall(StringBuilder stringBuilder, string pushenv)
        {
            //
        }

        private string GetParametricExpression(string define, ref int i)
        {
            if (i < define.Length-1)
            {
                if (define[i + 1] == '(')
                {
                    int curCheckIndex = i;
                    do
                    {
                        i++;
                        //TODO 寻找对称反括号 
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