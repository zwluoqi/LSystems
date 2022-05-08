using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSetting : ScriptableObject
{
    public ShapeType shapeType = ShapeType.SimpleNode;
    public MeshTopology meshTopology = MeshTopology.Triangles;
    public GenerateType generateType = GenerateType.Template;
    public float angle = 25;
    [Range(0.01f,10)]
    public float lengthFactor = 1;
    [Range(0,1)]
    public float widthIncrementFactor=0;
    [Range(1,30)]
    public int maxIter = 1;
    public Vector2 size = new Vector2(0.1f,1);
    public Vector2 defaultSize = new Vector2(1f,1);
    [Obsolete("旧版本,直接使用templateRules数组规则即可")]
    public string templateRule;
    public string[] templateRules = new string[0];
    public string[] constantDefines = new string[0];
    public string initRule;
    public PredefineShape[] predefineShapes = new PredefineShape[0];
}

[Serializable]
public class PredefineShape
{
    public char shapeKey;
    // public Mesh shape;
}

[Serializable]
public enum ShapeType
{
    SimpleNode,
    Tree,
    MergeSubMesh,
}

[Serializable]
public enum GenerateType
{
    Demo,
    FactalPlant,
    Template,
}
