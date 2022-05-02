using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class LSystemMesh : MonoBehaviour,ISettingUpdate
{

    public ShapeSetting shapeSetting;
    public ColorSetting colorSetting;

    public MeshFilter meshFilter;
    public MeshFilter[] subMeshFilter;
    private LSystemGenerate _lSystemGenerate = new LSystemGenerate();
    private Material LitMaterial;
    void GenerateMesh()
    {
        meshFilter = FillMeshFilter(meshFilter,"main",true);
        
        meshFilter.sharedMesh.Clear();
        var generateMeshData = _lSystemGenerate.Generate(shapeSetting);
        meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
        meshFilter.sharedMesh.vertices = generateMeshData.vector3s.ToArray();
        meshFilter.sharedMesh.triangles = generateMeshData.triangents.ToArray();
        meshFilter.sharedMesh.uv = generateMeshData.uvs.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateTangents();

        _InitSubMesh(generateMeshData.subMeshDatas.Count);
        for (int i = 0; i < generateMeshData.subMeshDatas.Count; i++)
        {
            var sub = this.subMeshFilter[i];
            sub.gameObject.SetActive(true);

            Mesh sharedMesh;
            generateMeshData.subMeshDatas[i].Normalize();
            sub.transform.localPosition = generateMeshData.subMeshDatas[i].centerPos;
            int[] triangles = GetTriangles(generateMeshData.subMeshDatas[i].vector3s,out var uvs);
            sharedMesh =  sub.sharedMesh;
            sharedMesh.Clear();
            sharedMesh.vertices = generateMeshData.subMeshDatas[i].vector3s.ToArray();
            sharedMesh.triangles = triangles;
            sharedMesh.uv = uvs;
            sharedMesh.RecalculateNormals();
            sharedMesh.RecalculateTangents();
        }
        for (int i = generateMeshData.subMeshDatas.Count; i < subMeshFilter.Length; i++)
        {
            this.subMeshFilter[i].gameObject.SetActive(false);
        }

    }

    private void _InitSubMesh(int count)
    {
        if (subMeshFilter == null)
        {
            subMeshFilter = new MeshFilter[count];
        }

        if (subMeshFilter.Length < count)
        {
            var tmp =  new MeshFilter[count];
            Array.Copy(subMeshFilter,tmp,subMeshFilter.Length);
            subMeshFilter = tmp;
        }
        for (int i = 0; i < count; i++)
        {
            var sub = _InitSubMesh0(i);
            this.subMeshFilter[i] = sub;
        }
    }

    private MeshFilter _InitSubMesh0(int index)
    {
        var sub = this.subMeshFilter[index];
        sub = FillMeshFilter(sub,"sub"+index,false);
        return sub;
    }

    private MeshFilter FillMeshFilter(MeshFilter sub,string names,bool treenode)
    {
        if (sub == null)
        {
            sub = (new GameObject(names)).AddComponent<MeshFilter>();
            Transform transform1;
            (transform1 = sub.transform).SetParent(this.transform);
            transform1.localScale = Vector3.one;
            transform1.localPosition = Vector3.zero;
            transform1.localRotation = Quaternion.identity;
        }

        if (LitMaterial == null)
        {
            LitMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        if (!sub.TryGetComponent<MeshRenderer>(out var renderer))
        {
            var render = sub.gameObject.AddComponent<MeshRenderer>();
            render.sharedMaterial = LitMaterial;
        }
        else
        {
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = LitMaterial;
            }
        }
        
        
        if (colorSetting != null)
        {
            if (treenode)
            {
                if (renderer.sharedMaterial.name != colorSetting.material.name)
                {
                    renderer.sharedMaterial = colorSetting.material;
                }
            }
            else
            {
                if (renderer.sharedMaterial.name != colorSetting.leafMaterial.name)
                {
                    renderer.sharedMaterial = colorSetting.leafMaterial;
                }
            }
        }

        if (sub.sharedMesh == null)
        {
            sub.sharedMesh = new Mesh {hideFlags = HideFlags.DontSave};
        }

        return sub;
    }

    public void UpdateSetting(ScriptableObject obj)
    {
        Debug.LogWarning(obj.name+" update");
        GenerateMesh();
    }

    private void OnValidate()
    {
        Debug.LogWarning(this.name+" OnValidate");
        GenerateMesh();
    }
    
    
    public static int[] GetTriangles(List<Vector3> vector3s,out Vector2[] uvs)
    {
        if (vector3s.Count == 6)
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
        else
        {
            uvs = null;
            return vector3s.Select(((vector3, i) => i)).ToArray();
                
        }
    }
}

