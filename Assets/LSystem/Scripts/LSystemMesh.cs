using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSystem.Scripts.FillShape;
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
        var generateMeshData = _lSystemGenerate.Generate(shapeSetting);

        FillMeshData(generateMeshData);

    }

    private void FillMeshData(GenerateMeshData generateMeshData)
    {
        
        meshFilter = FillMeshFilter(meshFilter,"main",true);
        _InitSubMesh(generateMeshData.subMeshDatas.Count+generateMeshData.subPredefineDatas.Count);

        IFillShape.FillMesh(shapeSetting, meshFilter, subMeshFilter,generateMeshData);
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
        if (!sub.TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            meshRenderer = sub.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = LitMaterial;
        }
        
        if (meshRenderer.sharedMaterial == null)
        {
            meshRenderer.sharedMaterial = LitMaterial;
        }
        
        
        if (colorSetting != null)
        {
            if (treenode)
            {
                if (meshRenderer.sharedMaterial.name != colorSetting.material.name)
                {
                    meshRenderer.sharedMaterial = colorSetting.material;
                }
            }
            else
            {
                if (meshRenderer.sharedMaterial.name != colorSetting.leafMaterial.name)
                {
                    meshRenderer.sharedMaterial = colorSetting.leafMaterial;
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
    
    
}

