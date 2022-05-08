using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSystem.Scripts.FillShape
{
    public abstract class IFillShape
    {
        
        protected MeshFilter meshFilter;
        protected  MeshFilter[] subMeshFilter;
        protected  ShapeSetting shapeSetting;
        protected  ColorSetting colorSetting;
        public void Update(ShapeSetting _shapeSetting,ColorSetting _colorSetting, MeshFilter _meshFilter,MeshFilter[] _subMeshFilter)
        {
            this.shapeSetting = _shapeSetting;
            this.colorSetting = _colorSetting;
            this.meshFilter = _meshFilter;
            this.subMeshFilter = _subMeshFilter;
        }
        
        public abstract void FillData(GenerateMeshData generateMeshData);
        
         protected int[] GetTriangles(List<Vector3> vector3s,out Vector2[] uvs)
            {
               
                if (vector3s.Count == 8)
                {
                    int[] triangles = new int[6*3];
                    uvs = new Vector2[vector3s.Count];
                    int i = 0;
                    triangles[i++] = 0;
                    triangles[i++] = 1;
                    triangles[i++] = 7;
                        
                    triangles[i++] = 1;
                    triangles[i++] = 2;
                    triangles[i++] = 3;
                        
                    triangles[i++] = 3;
                    triangles[i++] = 4;
                    triangles[i++] = 5;
                        
                    triangles[i++] = 5;
                    triangles[i++] = 6;
                    triangles[i++] = 7;
                    
                    triangles[i++] = 7;
                    triangles[i++] = 1;
                    triangles[i++] = 5;
                    
                    triangles[i++] = 1;
                    triangles[i++] = 3;
                    triangles[i++] = 5;
                        
                    uvs[0] = new Vector2(0,0.5f);
                    uvs[1] = new Vector2(1.0f/4.0f,1.0f/4.0f);
                    uvs[2] = new Vector2(2.0f/4.0f,0.0f);
                    uvs[3] = new Vector2(3.0f/4.0f,1.0f/4.0f);
                    uvs[4] = new Vector2(4.0f/4.0f,0.5f);
                    
                    uvs[5] = new Vector2(3.0f/4.0f,3.0f/4.0f);
                    uvs[6] = new Vector2(2.0f/4.0f,1);
                    uvs[7] = new Vector2(1.0f/4.0f,3.0f/4.0f);
                        
                    return triangles;
                }
                else if (vector3s.Count == 7)
                {
                    int[] triangles = new int[5*3];
                    uvs = new Vector2[vector3s.Count];
                    int i = 0;
                    triangles[i++] = 0;
                    triangles[i++] = 1;
                    triangles[i++] = 6;
                        
                    triangles[i++] = 1;
                    triangles[i++] = 2;
                    triangles[i++] = 6;
                        
                    triangles[i++] = 2;
                    triangles[i++] = 3;
                    triangles[i++] = 4;
                        
                    triangles[i++] = 4;
                    triangles[i++] = 5;
                    triangles[i++] = 2;
                    
                    triangles[i++] = 5;
                    triangles[i++] = 6;
                    triangles[i++] = 2;
                        
                    uvs[0] = new Vector2(0,0.5f);
                    uvs[1] = new Vector2(1.0f/3.0f,2/6.0f);
                    uvs[2] = new Vector2(2.0f/3.0f,3/6.0f);
                    uvs[3] = new Vector2(1,0.0f);
                    uvs[4] = new Vector2(1,1.0f);
                    uvs[5] = new Vector2(2.0f/3.0f,5/6.0f);
                    uvs[6] = new Vector2(1.0f/3.0f,4/6.0f);
                        
                    return triangles;
                }
                else if (vector3s.Count == 6)
                {
                    int[] triangles = new int[4*3];
                    uvs = new Vector2[vector3s.Count];
                    int i = 0;
                    triangles[i++] = 0;
                    triangles[i++] = 1;
                    triangles[i++] = 5;
                        
                    triangles[i++] = 5;
                    triangles[i++] = 1;
                    triangles[i++] = 2;
                        
                    triangles[i++] = 2;
                    triangles[i++] = 4;
                    triangles[i++] = 5;
                        
                    triangles[i++] = 4;
                    triangles[i++] = 2;
                    triangles[i++] = 3;
                        
                    uvs[0] = new Vector2(0,0.5f);
                    uvs[1] = new Vector2(1.0f/3.0f,0.0f);
                    uvs[2] = new Vector2(2.0f/3.0f,0.0f);
                    uvs[3] = new Vector2(1,0.5f);
                    uvs[4] = new Vector2(2.0f/3.0f,1.0f);
                    uvs[5] = new Vector2(1.0f/3.0f,1.0f);
                        
                    return triangles;
                }
                else if (vector3s.Count == 3)
                {
                    uvs = new Vector2[vector3s.Count];
                    uvs[0] = new Vector2(0,0);
                    uvs[1] = new Vector2(1,0);
                    uvs[2] = new Vector2(1,1);
                    return vector3s.Select(((vector3, i) => i)).ToArray();
                }
                else if (vector3s.Count == 4)
                {
                    int[] triangles = new int[2*3];
                    uvs = new Vector2[vector3s.Count];
                    int i = 0;
                    triangles[i++] = 0;
                    triangles[i++] = 1;
                    triangles[i++] = 2;
                        
                    triangles[i++] = 2;
                    triangles[i++] = 3;
                    triangles[i++] = 0;
                    
                        
                    uvs[0] = new Vector2(0,0.5f);
                    uvs[1] = new Vector2(0.5f,0.0f);
                    uvs[2] = new Vector2(1,0.5f);
                    uvs[3] = new Vector2(.5f,.5f);
                        
                    return triangles;
                }
                else
                {
                    uvs = null;
                    return vector3s.Select(((vector3, i) => i)).ToArray();
                }
            }
            
         protected  int[] ConvertTriangleToLine(int[] triangles)
            {
                var lines = new int[(triangles.Length / 3)*6];
                for (int i = 0; i < triangles.Length/3; i++)
                {
                    lines[0 + i * 6] = triangles[i * 3 + 0];
                    lines[1 + i * 6] = triangles[i * 3 + 1];
                    lines[2 + i * 6] = triangles[i * 3 + 0];
                    lines[3 + i * 6] = triangles[i * 3 + 2];
                    lines[4 + i * 6] = triangles[i * 3 + 1];
                    lines[5 + i * 6] = triangles[i * 3 + 2];
                }
        
                return lines;
            }

            public static void FillMesh(ShapeSetting shapeSetting,ColorSetting colorSetting,
                MeshFilter meshFilter, MeshFilter[] subMeshFilter,GenerateMeshData generateMeshData)
            {
                IFillShape fillShape = CreateFillShape(shapeSetting.shapeType);
                fillShape.Update(shapeSetting,colorSetting,meshFilter,subMeshFilter);
                fillShape.FillData(generateMeshData);
            }

            private static IFillShape CreateFillShape(ShapeType shapeSettingShapeType)
            {
                switch (shapeSettingShapeType)
                {
                    case ShapeType.SimpleNode:
                        return new SimpleCubeFillShape();
                    case ShapeType.MergeSubMesh:
                        return new MergeSubFillShape();
                    case ShapeType.Tree:
                        return new TreeFillShape();
                    default:
                        throw new NotImplementedException("SS");
                        break;
                }
            }
    }
}