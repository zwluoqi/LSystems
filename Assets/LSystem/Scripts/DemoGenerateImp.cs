using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem.Scripts
{
    public class DemoGenerateImp : IGenerateImp
    {
        
        public override List<Vector3> Generate(ShapeSetting shapeSetting)
        {
            List<Vector3> vector3s = new List<Vector3>();
        
            //define
            Action<int> F = delegate(int iter)
            {
                UpdateRect(shapeSetting.size);
                AddCell(ref vector3s);
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
                Rotation(shapeSetting.angle);
                F(iter+1);
                A(iter+1);
                PopEvn();
            
                PushEnv();
                Rotation(-shapeSetting.angle);
                F(iter+1);
                A(iter+1);
                PopEvn();
            };

            //init
            {
                F(1);
                A(1);
            }
        
            return vector3s;
        }
    }
}