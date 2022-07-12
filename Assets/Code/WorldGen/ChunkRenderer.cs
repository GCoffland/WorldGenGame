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
    private bool[] borderBlockMapDirtyBits = Enumerable.Repeat(true, 6).ToArray();

    void Awake()
    {
        SetDefaultMesh();
        chunk.model.OnModelChanged += onModelChanged;
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        for (int n = 0; n < 6; n++)
        {
            if (borderBlockMaps[n].IsCreated) borderBlockMaps[n].Dispose();
        }
    }

    private void onModelChanged(BoundsInt bounds)
    {
        if (enabled)
            _ = GenerateMesh();
        foreach (DIRECTION d in GetBordersContained(bounds))
        {
            updateNeighborBorderBlockmap(d);
        }
    }

    public List<DIRECTION> GetBordersContained(BoundsInt container)
    {
        List<DIRECTION> dirs = new List<DIRECTION>();
        for (int i = 0; i < 3; i++)
        {
            if (container.min[i] <= 0)
            {
                dirs.Add((DIRECTION)(i * 2));
            }
            if (container.max[i] >= WorldGenerationGlobals.ChunkSize[i])
            {
                dirs.Add((DIRECTION)(i * 2 + 1));
            }
        }
        return dirs;
    }

    private void updateNeighborBorderBlockmap(DIRECTION dir)
    {
        if(chunk.neighbors[(int)dir] != null)
        {
            chunk.neighbors[(int)dir].renderer.borderBlockMapDirtyBits[(int)dir / 2 * 2 + 1 - (int)dir % 2] = true;
            if (chunk.neighbors[(int)dir].renderer.enabled)
                _ = chunk.neighbors[(int)dir].renderer.GenerateMesh();
        }
    }

    public void TryFetchBorderBlockmapsIfNeeded()
    {
        for (int n = 0; n < StaticDefinitions.Directions.Length; n++)
        {
            if (!borderBlockMaps[n].IsCreated)
            {
                borderBlockMaps[n] = new NativeArray<uint>(WorldGenerationGlobals.ChunkSideAreas[n], Allocator.Persistent);
            }
            if (borderBlockMapDirtyBits[n] && chunk.neighbors[n] != null && chunk.neighbors[n].model.blockMap.IsCreated)
            {
                chunk.model.FetchBorderBlockmap((DIRECTION)n, ref borderBlockMaps[n]);
                borderBlockMapDirtyBits[n] = false;
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
        if (!chunk.model.blockMap.IsCreated)
            throw new InvalidOperationException("Cannot generate a mesh for a chunk without a valid model");
        TryFetchBorderBlockmapsIfNeeded();
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(chunk.model.blockMap, borderBlockMaps);
        meshFilter.mesh.Clear();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
    }
}
