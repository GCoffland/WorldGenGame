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

    private NativeArray<uint>[] borderBlockMaps = new NativeArray<uint>[6];
    private bool[] borderBlockMapIsClean = new bool[6];
    private Task meshGenTask;

    void Awake()
    {
        SetDefaultMesh();
        chunk.model.OnBlockChanged += genMeshHelper;
        chunk.model.OnModelGenerated += onModelGenerated;
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

    private void OnDestroy()
    {
        for (int n = 0; n < 6; n++)
        {
            if (borderBlockMaps[n].IsCreated) borderBlockMaps[n].Dispose();
        }
    }

    private void onModelGenerated()
    {
        for (int i = 0; i < StaticDefinitions.Directions.Length; i++)  // Nofiy neighbors
        {
            if (chunk.neighbors[i] != null)
            {
                notifyNeighborOfChange((DIRECTION)i);
            }
        }
    }

    private void genMeshHelper(int x, int y, int z)
    {
        modelChangedThisFrame = true;
        if (ChunkModel.IsBorderBlock(new Vector3Int(x, y, z), out DIRECTION dir) && chunk.neighbors[(int)dir] != null)
        {
            notifyNeighborOfChange(dir);
        }
    }

    private void notifyNeighborOfChange(DIRECTION dir)
    {
        chunk.neighbors[(int)dir].renderer.borderBlockMapIsClean[(int)dir + 1 - 2 * ((int)dir % 2)] = false;
        _ = chunk.neighbors[(int)dir].renderer.GenerateMesh();
    }

    public void TryFetchBorderBlockmaps()
    {
        for (int n = 0; n < StaticDefinitions.Directions.Length; n++)
        {
            if (!borderBlockMaps[n].IsCreated)
            {
                borderBlockMaps[n] = new NativeArray<uint>(WorldGenerationGlobals.ChunkSideAreas[n], Allocator.Persistent);
            }
            if (!borderBlockMapIsClean[n] && chunk.neighbors[n] != null && chunk.neighbors[n].model.blockMap.IsCreated)
            {
                chunk.model.FetchBorderBlockmap((DIRECTION)n, ref borderBlockMaps[n]);
                borderBlockMapIsClean[n] = true;
            }
        }
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
        TryFetchBorderBlockmaps();
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(chunk.model.blockMap, borderBlockMaps);
        meshFilter.mesh.Clear();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
    }
}
