using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WorldGeneration;
using Unity.Jobs;
using System;
using System.Linq;

public class ChunkRenderer : MonoBehaviour
{
    [SerializeField]
    private Chunk chunk;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private Mesh defaultMesh;

    private Task meshGenTask;

    void Awake()
    {
        SetDefaultMesh();
        chunk.model.OnBlockChanged += genMeshHelper;
    }

    private bool modelChangedThisFrame = false;

    private void LateUpdate()
    {
        if (modelChangedThisFrame)
        {
            meshGenTask = GenerateMesh();
            modelChangedThisFrame = false;
        }
    }

    private void genMeshHelper(int x, int y, int z)
    {
        modelChangedThisFrame = true;
    }

    public void SetDefaultMesh()
    {
        meshFilter.mesh = defaultMesh;
        Vector3[] verts = meshFilter.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)  // make mesh appear to be a large cube
        {
            verts[i] += Vector3.one / 2;
            verts[i] = Vector3.Scale(verts[i], WorldGenerationGlobals.ChunkSize);
        }
        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.bounds = new Bounds(chunk.bounds.size / 2, chunk.bounds.size);
    }

    public async Task GenerateMesh()
    {
        Debug.Log("Generating Mesh for Chunk: " + chunk.index);
        chunk.model.TryFetchBorderBlockmaps();
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(chunk.model.blockMap, chunk.model.borderBlockMaps);
        meshFilter.mesh.Clear();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
    }
}
