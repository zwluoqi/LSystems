using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class LSystemMesh : MonoBehaviour,ISettingUpdate
{

    public ShapeSetting shapeSetting;


    public MeshFilter meshFilter;
    private LSystemGenerate _lSystemGenerate = new LSystemGenerate();
    
    void GenerateMesh()
    {
        _InitMesh();
        
        meshFilter.sharedMesh.Clear();
        var generateMeshData = _lSystemGenerate.Generate(shapeSetting);
        meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
        meshFilter.sharedMesh.vertices = generateMeshData.vector3s.ToArray();
        meshFilter.sharedMesh.triangles = generateMeshData.triangents.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateTangents();
        
    }

    private void _InitMesh()
    {
        if (meshFilter == null)
        {
            meshFilter = (new GameObject("system")).AddComponent<MeshFilter>();
            meshFilter.transform.SetParent(this.transform);
            meshFilter.transform.localScale = Vector3.one;
            meshFilter.transform.localPosition = Vector3.zero;
            meshFilter.transform.localRotation = Quaternion.identity;
        }

        if (!meshFilter.TryGetComponent<MeshRenderer>(out var renderer))
        {
            var render = meshFilter.gameObject.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        else
        {
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
        }

        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh {hideFlags = HideFlags.DontSave};
        }
    }

    public void UpdateSetting(ScriptableObject obj)
    {
        Debug.LogWarning(obj.name+" update");
        GenerateMesh();
    }
}

