using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem.Scripts
{
    public class FactalPlantGenerateImp : IGenerateImp
    {
        public override GenerateMeshData Generate(ShapeSetting shapeSetting)
        {
            GenerateMeshData generateMeshData = new GenerateMeshData();
        
            // 生成规则：1. X→F+[[X]-X]-F[-FX]+X; 2.F→FF
            //define
            Action<int> F = null;
            F = delegate(int iter)
            {
                if (iter > shapeSetting.maxIter)
                {
                    UpdateRect(shapeSetting);
                    AddCell(ref generateMeshData);
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
        
            return generateMeshData;
        }
    }
}