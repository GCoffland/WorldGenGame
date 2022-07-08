using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkRenderer))]
public class ChunkRundererEditor : Editor
{
    new ChunkRenderer target;

    private void OnEnable()
    {
        target = base.target as ChunkRenderer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate Mesh"))
        {
            _ = target.GenerateMesh();
        }
    }
}
