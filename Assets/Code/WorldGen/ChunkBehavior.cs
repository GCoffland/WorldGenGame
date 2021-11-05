﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading;
using System.Threading.Tasks;
using WorldGeneration;
using Unity.Jobs;

public class ChunkBehavior : MonoBehaviour
{
    private BoundsInt bounds;

    private MeshGenerator meshGenerator;
    private MeshFilter meshFilter;
    private enum SpawnedState {
        Uninitialized,
        Spawning,
        Spawned
    };
    private SpawnedState state = SpawnedState.Uninitialized;

    NativeArray<VOXELTYPE> blockMap;

    void Awake()
    {
        meshGenerator = GetComponent<MeshGenerator>();
        meshFilter = GetComponent<MeshFilter>();
        if (state == SpawnedState.Uninitialized)
        {
            Vector3[] verts = meshFilter.mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] += Vector3.one / 2;
                verts[i] = Vector3.Scale(verts[i], Constants.ChunkSize);
            }
            meshFilter.mesh.vertices = verts;
        }
        meshFilter.mesh.RecalculateBounds();
    }

    private void OnBecameVisible()
    {
        if(state == SpawnedState.Uninitialized)
        {
            _ = Spawn();
        }
    }

    void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
    }

    public async Task Spawn()
    {
        state = SpawnedState.Spawning;
        bounds = new BoundsInt(transform.position.ToVector3Int(), Constants.ChunkSize);
        blockMap = new NativeArray<VOXELTYPE>((bounds.size.x + 2) * (bounds.size.y + 2) * (bounds.size.z + 2), Allocator.Persistent);
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
        }
        await Generate();
        state = SpawnedState.Spawned;
    }

    private async Task Generate()
    {
        await ChunkModelGenerator.GenerateBlockmap(blockMap, bounds.size, bounds.position);
        await MeshGenerator.GenerateMeshData(blockMap, meshFilter.mesh);
        WorldBehavior.instance.chunkCurrentlyGenerating = false;
    }

    public void removeBlockAt(Vector3Int localpos)
    {
        Debug.Log("Tried to remove a block at " + localpos);
    }
}
