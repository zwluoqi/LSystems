using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ColorSetting : ScriptableObject
{

    public Material material;
    public Material leafMaterial;

    public TemplateShape[] templateShapes = new TemplateShape[0];
}

[Serializable]
public class TemplateShape
{
    public char shapeKey;
    public int shapeIndex;
    public Mesh sharedMesh;
    public Material material;
}
