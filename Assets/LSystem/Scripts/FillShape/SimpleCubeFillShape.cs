using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LSystem.Scripts.FillShape
{
    public class SimpleCubeFillShape:IFillShape
    {
        //

        public override void FillData(GenerateMeshData generateMeshData)
        {
            Mesh mesh;
            (mesh = meshFilter.sharedMesh).Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = generateMeshData.vector3s.ToArray();
            var mainTriangles = generateMeshData.triangents.ToArray();
            mesh.uv = generateMeshData.uvs.ToArray();
            if (shapeSetting.meshTopology == MeshTopology.Lines)
            {
                var lines = ConvertTriangleToLine(mainTriangles);
                mesh.SetIndices(lines, MeshTopology.Lines, 0);
            }
            else
            {
                mesh.triangles = mainTriangles;
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
            }

            for (int i = 0; i < generateMeshData.subMeshDatas.Count; i++)
            {
                var sub = this.subMeshFilter[i];
                sub.gameObject.SetActive(true);

                generateMeshData.subMeshDatas[i].Normalize();
                sub.transform.localPosition = generateMeshData.subMeshDatas[i].centerPos;
                int[] triangles = GetTriangles(generateMeshData.subMeshDatas[i].vector3s,out var uvs);
                var sharedMesh = sub.sharedMesh;
                sharedMesh.Clear();
                sharedMesh.vertices = generateMeshData.subMeshDatas[i].vector3s.ToArray();
                sharedMesh.uv = uvs;
                if (shapeSetting.meshTopology == MeshTopology.Lines)
                {
                    var lines = ConvertTriangleToLine(triangles);
                    sharedMesh.SetIndices(lines, MeshTopology.Lines, 0);
                }
                else
                {
                    sharedMesh.triangles = triangles;
                    sharedMesh.RecalculateNormals();
                    sharedMesh.RecalculateTangents();
                }
            }

            for (int i = 0; i < generateMeshData.subPredefineDatas.Count; i++)
            {
                var sub = this.subMeshFilter[generateMeshData.subMeshDatas.Count+i];
                sub.gameObject.SetActive(true);
                
                sub.transform.localPosition = generateMeshData.subPredefineDatas[i].pos;
                sub.transform.forward = Vector3.Cross(generateMeshData.subPredefineDatas[i].right,
                    generateMeshData.subPredefineDatas[i].up);
                
                var sharedMesh = sub.sharedMesh;
                sub.sharedMesh = generateMeshData.subPredefineDatas[i].shareMesh;
            }

            for (int i = generateMeshData.subMeshDatas.Count + generateMeshData.subPredefineDatas.Count; i < subMeshFilter.Length; i++)
            {
                if (subMeshFilter[i] == null)
                {
                    break;
                }
                this.subMeshFilter[i].gameObject.SetActive(false);
            }
        }
    }
}