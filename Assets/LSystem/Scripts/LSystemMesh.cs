using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class LSystemMesh : MonoBehaviour,ISettingUpdate
{

    public ShapeSetting shapeSetting;


    public MeshFilter meshFilter;
    public MeshFilter[] subMeshFilter;
    private LSystemGenerate _lSystemGenerate = new LSystemGenerate();
    
    void GenerateMesh()
    {
        FillMeshFilter(meshFilter,"main");
        
        meshFilter.sharedMesh.Clear();
        var generateMeshData = _lSystemGenerate.Generate(shapeSetting);
        meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
        meshFilter.sharedMesh.vertices = generateMeshData.vector3s.ToArray();
        meshFilter.sharedMesh.triangles = generateMeshData.triangents.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateTangents();

        _InitSubMesh(generateMeshData.subMeshDatas.Count);
        for (int i = 0; i < generateMeshData.subMeshDatas.Count; i++)
        {
            var sub = this.subMeshFilter[i];
            sub.gameObject.SetActive(true);
            sub.sharedMesh.vertices = generateMeshData.subMeshDatas[i].vector3s.ToArray();
            sub.sharedMesh.triangles =
                generateMeshData.subMeshDatas[i].vector3s.Select(((vector3, i1) => i1)).ToArray();
            sub.sharedMesh.RecalculateNormals();
            sub.sharedMesh.RecalculateTangents();
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
        sub = FillMeshFilter(sub,"sub"+index);
        return sub;
    }

    private MeshFilter FillMeshFilter(MeshFilter sub,string names)
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

        if (!sub.TryGetComponent<MeshRenderer>(out var renderer))
        {
            var render = sub.gameObject.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        else
        {
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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
}

