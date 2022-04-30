using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem.Scripts
{
    public class DemoGenerateImp : IGenerateImp
    {
        
        public override GenerateMeshData Generate(ShapeSetting shapeSetting)
        {
            GenerateMeshData generateMeshData = new GenerateMeshData();
        
            //define
            Action<int> F = delegate(int iter)
            {
                UpdateRect(shapeSetting);
                AddCell(ref generateMeshData);
                UpdatePos(shapeSetting.size);
            };
            Action<int> A = null;
            // 生成规则：A=[+FA][-FA] 
            A = delegate(int iter)
            {
                if (iter > shapeSetting.maxIter)
                {
                    return;
                }

                PushEnv();
                RotationF(shapeSetting);
                F(iter+1);
                A(iter+1);
                PopEvn();
            
                PushEnv();
                RotationB(shapeSetting);
                F(iter+1);
                A(iter+1);
                PopEvn();
            };

            //init
            {
                F(1);
                A(1);
            }
        
            return generateMeshData;
        }

    }
}