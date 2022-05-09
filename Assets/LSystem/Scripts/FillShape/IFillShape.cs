using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.MeshTools;

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
                if (vector3s.Count < 3)
                {
                    throw new NotImplementedException("not support vertex small 3");
                }

                if (vector3s.Count > 3)
                {
                    var DelaunayTriangles = Delaunay3DTools.Delaunay3DPoint(vector3s);
                    uvs = DelaunayTriangles.Item2;
                    return DelaunayTriangles.Item1;
                }

                if (vector3s.Count == 8)
                {
                    return PlaneGemotryTools.GetTriangles8(vector3s, out uvs);
                }
                else if (vector3s.Count == 7)
                {
                    return PlaneGemotryTools.GetTriangles7(vector3s, out uvs);
                }
                else if (vector3s.Count == 6)
                {
                    return PlaneGemotryTools.GetTriangles6(vector3s, out uvs);
                }
                else if (vector3s.Count == 4)
                {
                    return PlaneGemotryTools.GetTriangles4(vector3s, out uvs);
                }
                else if (vector3s.Count == 3)
                {
                    return PlaneGemotryTools.GetTriangles3(vector3s, out uvs);
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