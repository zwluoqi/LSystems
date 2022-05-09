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

            for (int i = 0; i < subMeshFilter.Length; i++)
            {
                if (subMeshFilter[i] == null)
                {
                    break;
                }
                this.subMeshFilter[i].gameObject.SetActive(false);
            }
            
            int meshIndex = 0;
            Dictionary<int,SaveMeshNode> saveMeshNodes = new Dictionary<int, SaveMeshNode>();
            for (int i = 0; i < generateMeshData.subMeshDatas.Count; i++)
            {
                var sub = this.subMeshFilter[meshIndex];
                if (generateMeshData.subMeshDatas[i].vector3s.Count < 3)
                {
                    continue;
                }

                meshIndex++;
                sub.gameObject.SetActive(true);

                generateMeshData.subMeshDatas[i].Normalize();
                sub.transform.localPosition = generateMeshData.subMeshDatas[i].centerPos;
                
                if (!saveMeshNodes.TryGetValue(generateMeshData.subMeshDatas[i].vector3s.Count, out var saveMeshNode))
                {
                    int[] triangles = GetTriangles(generateMeshData.subMeshDatas[i].vector3s,out var uvs);
                    saveMeshNode = new SaveMeshNode();
                    saveMeshNode.triangles = triangles;
                    saveMeshNode.uvs = uvs;
                    saveMeshNodes.Add(generateMeshData.subMeshDatas[i].vector3s.Count,saveMeshNode);
                }
                var sharedMesh = sub.sharedMesh;
                sharedMesh.name = generateMeshData.subMeshDatas[i].vector3s.Count + "";
                sharedMesh.Clear();
                sharedMesh.vertices = generateMeshData.subMeshDatas[i].vector3s.ToArray();
                sharedMesh.uv = saveMeshNode.uvs;
                if (shapeSetting.meshTopology == MeshTopology.Lines)
                {
                    var lines = ConvertTriangleToLine(saveMeshNode.triangles);
                    sharedMesh.SetIndices(lines, MeshTopology.Lines, 0);
                }
                else
                {
                    sharedMesh.triangles = saveMeshNode.triangles;
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

                var shapeKey = generateMeshData.subPredefineDatas[i].shapeKey;
                var shapeIndex = (int) generateMeshData.subPredefineDatas[i].preParam;
                var shape = GetColorTemplateShape(shapeKey, shapeIndex);
                var useDefalutShape = false;
                if (shape == null)
                {
                    shape = GetColorTemplateShape(shapeKey, 0);
                    useDefalutShape = true;
                }
                if (shape != null)
                {
                    sub.sharedMesh = UnityEngine.Object.Instantiate(shape.sharedMesh);
                    sub.GetComponent<MeshRenderer>().sharedMaterial = shape.material;
                    
                    if (shape.iterScale && useDefalutShape)
                    {
                        var preParam = generateMeshData.subPredefineDatas[i].preParam;
                        var scale = Vector3.one;
                        if (preParam > 0)
                        {
                            var scaleFactor = 1/preParam;
                            scale = (new Vector3(scaleFactor, scaleFactor, scaleFactor));
                        }
                        sub.transform.localScale =  shape.scale*scale;
                    }
                    else
                    {
                        sub.transform.localScale = Vector3.one* shape.scale;
                    }
                }
                else
                {
                    Debug.LogError($"not find shape:{shapeKey} key:{shapeIndex}");
                }

                
            }
        }

        public class SaveMeshNode
        {
            public int[] triangles;
            public Vector2[] uvs;
        }

        private TemplateShape GetColorTemplateShape(char shapeKey, int preParam)
        {
            for (int i = 0; i < colorSetting.templateShapes.Length; i++)
            {
                var temp = colorSetting.templateShapes[i];
                if (temp.shapeKey == shapeKey && temp.shapeIndex == preParam)
                {
                    return temp;
                }
            }

            return null;
        }
    }
}