using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSetting : ScriptableObject
{
    public GenerateType generateType;
    // public float rotationStrength = 1;
    // public float rotationFrequency = 1;
    public float angle = 25;
    [Range(0.01f,10)]
    public float lengthFactor = 1;
    [Range(0,1)]
    public float widthIncrementFactor=0;
    [Range(1,10)]
    public int maxIter = 1;
    public Vector2 size = new Vector2(0.1f,1);
    [Obsolete("旧版本,直接使用templateRules数组规则即可")]
    public string templateRule;
    public string[] templateRules = new string[0];
    public string[] constantDefines = new string[0];
    public string initRule;
    
}

[Serializable]
public enum GenerateType
{
    Demo,
    FactalPlant,
    Template,
}
