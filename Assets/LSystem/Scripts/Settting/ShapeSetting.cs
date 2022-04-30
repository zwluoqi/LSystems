using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSetting : ScriptableObject
{
    public bool toggle;
    public float frequency;
    public GenerateType generateType;
    public float angle = 25;
    public int maxIter = 1;
    public Vector2 size = new Vector2(0.1f,1);
    public string templateRule;
    public string initRule;
    // public List<string> templateParams;
}

[Serializable]
public enum GenerateType
{
    Demo,
    FactalPlant,
    Template,
}
