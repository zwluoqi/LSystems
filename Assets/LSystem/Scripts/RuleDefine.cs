using System;
using System.Collections.Generic;
using LSystem.Scripts.Expression;

namespace LSystem.Scripts
{
public class RuleDefine
    {
        public class BranchRuleDefine
        {
            /// <summary>
            /// 条件表达式
            /// </summary>
            public string conditonExpression="";
            /// <summary>
            /// 条件概率
            /// </summary>
            public float radio = 1;
            /// <summary>
            /// 函数回调
            /// </summary>
            public Action<int,ParamStackEnv> publicAction;

            public BranchRuleDefine(string condition, Action<int, ParamStackEnv> action)
            {
                this.conditonExpression = condition;
                this.publicAction = action;
            }

            public BranchRuleDefine(Action<int, ParamStackEnv> action)
            {
                this.publicAction = action;
            }
        }
        /// <summary>
        /// 函数名
        /// </summary>
        public char key;
        /// <summary>
        /// 函数参数
        /// </summary>
        public string[] paramerrics = new string[4];

        //分支回调
        List<BranchRuleDefine> branches = new List<BranchRuleDefine>();

        public bool AttemptAction(int iter,ParamStackEnv paramStackEnv,string paramExpression)
        {
            ParamStackEnv paramStackEnv0 = paramStackEnv.Clone();
            var expressions = paramExpression.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < expressions.Length; i++)
            {
                paramStackEnv0.SetParamerrics(paramerrics[i], Expresssion.ParseExpression(paramStackEnv,expressions[i]));
            }
            // publicAction(iter, paramStackEnv0);
            foreach (var branch in branches)
            {
                if (branch.radio < 1)
                {
                    // branch.publicAction(iter, paramStackEnv0);
                    // return true;
                    // break;
                    throw new NotImplementedException("SSSSSSS");
                }

                if (string.IsNullOrEmpty(branch.conditonExpression))
                {
                    branch.publicAction(iter, paramStackEnv0);
                    return true;
                    break;
                }
                var pass = Expresssion.ParseExpression(paramStackEnv0, branch.conditonExpression);
                if (pass.Value>0)
                {
                    branch.publicAction(iter, paramStackEnv0);
                    return true;
                    break;
                }
            }

            return false;
        }

        public RuleDefine(char key, string[] paramerrics)
        {
            this.key = key;
            Array.Copy(paramerrics,this.paramerrics,this.paramerrics.Length);
        }
        public RuleDefine(char key)
        {
            this.key = key;
        }
        

        public void AddBranch(string conditionExpression, Action<int, ParamStackEnv> action)
        {
            BranchRuleDefine branchRuleDefine = new BranchRuleDefine(conditionExpression,action);
            branches.Add(branchRuleDefine);
        }
        public void AddBranch(Action<int, ParamStackEnv> action)
        {
            BranchRuleDefine branchRuleDefine = new BranchRuleDefine(action);
            branches.Add(branchRuleDefine);
        }
    }
}