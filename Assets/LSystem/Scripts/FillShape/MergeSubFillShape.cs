using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LSystem.Scripts.FillShape
{
    public class MergeSubFillShape:IFillShape
    {


        public override void FillData(GenerateMeshData generateMeshData)
        {
            for (int i = 0; i < subMeshFilter.Length; i++)
            {
                if (subMeshFilter[i] == null)
                {
                    break;
                }
                this.subMeshFilter[i].gameObject.SetActive(false);
            }
            
            Mesh mesh;
            (mesh = meshFilter.sharedMesh).Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            List<Vector3> mergeVector3S = new List<Vector3>(128);
            List<Vector2> mergeUVs = new List<Vector2>(128);
            List<int> mergeTrangles = new List<int>(256);
            foreach (var subMeshData in generateMeshData.subMeshDatas)
            {
                int[] triangles = GetTriangles(subMeshData.vector3s,out var uvs);
                var startVertexIndex = mergeVector3S.Count;
                for (int i = 0; i < triangles.Length; i++)
                {
                    mergeTrangles.Add(startVertexIndex+triangles[i]);    
                }
                mergeVector3S.AddRange(subMeshData.vector3s);
                mergeUVs.AddRange(uvs);
            }
            mesh.vertices = mergeVector3S.ToArray();
            mesh.uv = mergeUVs.ToArray();
            if (shapeSetting.meshTopology == MeshTopology.Lines)
            {
                var lines = ConvertTriangleToLine(mergeTrangles.ToArray());
                mesh.SetIndices(lines, MeshTopology.Lines, 0);
            }
            else
            {
                mesh.triangles = mergeTrangles.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }
            
        }

        private Vector2[] GetUVsXYaxis(List<Vector3> vector3S)
        {
            MinMax x = new MinMax();
            MinMax y = new MinMax();
            for (int i = 0; i < vector3S.Count; i++)
            {
                x.AddValue(vector3S[i].x);
                y.AddValue(vector3S[i].y);
            }
            Vector2[] uvs  = new Vector2[vector3S.Count];
            for (int i = 0; i < vector3S.Count; i++)
            {
                uvs[i] = new Vector2(
                    Mathf.InverseLerp(y.min, y.max, vector3S[i].y),Mathf.InverseLerp(x.min, x.max, vector3S[i].x));
            }

            return uvs;
        }
    }
}