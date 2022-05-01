using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem.Scripts
{
    public class FactalPlantGenerateImp : IGenerateImp
    {
        public override void Generate(ShapeSetting shapeSetting)
        {
            // 生成规则：1. X→F+[[X]-X]-F[-FX]+X; 2.F→FF
            //define
            Action<int> F = null;
            F = delegate(int iter)
            {
                if (iter > shapeSetting.maxIter)
                {
                    AddCell(shapeSetting);
                    UpdatePos(shapeSetting);
                }
                else
                {
                    F(iter+1);
                    F(iter+1);
                }
            };
            Action<int> A = null;
            A = delegate(int iter)
            {
                if (iter > shapeSetting.maxIter)
                {
                    return;
                }
                // 生成规则：1. X→F+[[X]-X]-F[-FX]+X; 2.F→FF

                F(iter+1);
                RotationF(shapeSetting);
                PushEnv();
                PushEnv();
                A(iter+1);
                PopEvn();
                RotationB(shapeSetting);
                A(iter+1);
                PopEvn();
                RotationB(shapeSetting);
                F(iter+1);
                PushEnv();
                RotationB(shapeSetting);
                F(iter+1);
                A(iter+1);
                PopEvn();
                RotationF(shapeSetting);
                A(iter+1);
            };

            //init
            {
                A(1);
            }
        
            return;
        }
    }
}