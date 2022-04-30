using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(LSystemMesh))]
public class LSystemMeshEditor : Editor
{
    private SettingEditor<LSystemMesh> shapeEdirot;

    private void OnEnable()
    {
        shapeEdirot = new SettingEditor<LSystemMesh>();
        shapeEdirot.OnEnable(this);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        shapeEdirot.OnInspectorGUI(this);
    }
}
