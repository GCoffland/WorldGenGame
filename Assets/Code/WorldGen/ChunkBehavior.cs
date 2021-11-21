using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading;
using System.Threading.Tasks;
using WorldGeneration;
using Unity.Jobs;
using System;

public class ChunkBehavior : MonoBehaviour
{
    public static event Action<ChunkBehavior> onAnyChunkStateChanged;
    public event Action onChunkStateChanged;

    public SpawnedState state
    {
        get
        {
            return _state;
        }
        set
        {
            if(value == _state)
            {
                return;
            }
            _state = value;
            onAnyChunkStateChanged?.Invoke(this);
            onChunkStateChanged?.Invoke();
        }
    }
    public BoundsInt bounds
    {
        get;
        private set;
    }

    public enum SpawnedState
    {
        Uninitialized,
        Initialized,
        Spawning,
        Spawned
    };

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private SpawnedState _state;

    NativeArray<VOXELTYPE> blockMap;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();        // assign fields
        meshCollider = GetComponent<MeshCollider>();

        if (state == SpawnedState.Uninitialized)        // make mesh appear to be a large cube
        {
            Vector3[] verts = meshFilter.mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] += Vector3.one / 2;
                verts[i] = Vector3.Scale(verts[i], Constants.ChunkSize);
            }
            meshFilter.mesh.vertices = verts;
        }
        bounds = new BoundsInt(transform.position.ToVector3Int(), Constants.ChunkSize);
        meshFilter.mesh.bounds = new Bounds(bounds.center, bounds.size);

        state = SpawnedState.Initialized;
    }

    private void OnBecameVisible()
    {
        if(state <= SpawnedState.Initialized)
        {
            _ = Spawn();
        }
    }

    private void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
    }

    public async Task Spawn()
    {
        state = SpawnedState.Spawning;
        blockMap = new NativeArray<VOXELTYPE>((bounds.size.x + 2) * (bounds.size.y + 2) * (bounds.size.z + 2), Allocator.Persistent);
        meshFilter.sharedMesh = new Mesh();
        await Generate();
        state = SpawnedState.Spawned;
    }

    private async Task Generate()
    {
        await ChunkModelGenerator.GenerateBlockmap(blockMap, bounds.position);
        await MeshGenerator.GenerateMeshData(blockMap, meshFilter.mesh);
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
}
