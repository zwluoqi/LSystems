using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem.Scripts
{
    public class FactalPlantGenerateImp : IGenerateImp
    {
        public override List<Vector3> Generate(ShapeSetting shapeSetting)
        {
            List<Vector3> vector3s = new List<Vector3>();
        
            // 生成规则：1. X→F+[[X]-X]-F[-FX]+X; 2.F→FF
            //define
            Action<int> F = null;
            F = delegate(int iter)
            {
                if (iter > shapeSetting.maxIter)
                {
                    UpdateRect(shapeSetting.size);
                    AddCell(ref vector3s);
                    UpdatePos(shapeSetting.size);
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
                Rotation(shapeSetting.angle);
                PushEnv();
                PushEnv();
                A(iter+1);
                PopEvn();
                Rotation(-shapeSetting.angle);
                A(iter+1);
                PopEvn();
                Rotation(-shapeSetting.angle);
                F(iter+1);
                PushEnv();
                Rotation(-shapeSetting.angle);
                F(iter+1);
                A(iter+1);
                PopEvn();
                Rotation(shapeSetting.angle);
                A(iter+1);
            };

            //init
            {
                A(1);
            }
        
            return vector3s;
        }
    }
}