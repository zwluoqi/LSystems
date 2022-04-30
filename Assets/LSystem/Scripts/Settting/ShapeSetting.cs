using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSetting : ScriptableObject
{
    public GenerateType generateType;
    public float rotationFrequency = 1;
    public float angle = 25;
    [Range(1,10)]
    public int maxIter = 1;
    public Vector2 size = new Vector2(0.1f,1);
    public string templateRule;
    public string initRule;
    
}

[Serializable]
public enum GenerateType
{
    Demo,
    FactalPlant,
    Template,
}
