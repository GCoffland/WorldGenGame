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

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private Mesh defaultMesh;
    private SpawnedState _state;

    NativeArray<uint> blockMap;

    void Awake()
    {
        SetDefaultMesh();

        state = SpawnedState.Initialized;
    }

    private void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
    }

    private void SetDefaultMesh()
    {
        meshFilter.mesh = defaultMesh;
        Vector3[] verts = meshFilter.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)  // make mesh appear to be a large cube
        {
            verts[i] += Vector3.one / 2;
            verts[i] = Vector3.Scale(verts[i], WorldGenerationGlobals.ChunkSize);
        }
        meshFilter.mesh.vertices = verts;
        bounds = new BoundsInt(transform.position.ToVector3Int(), WorldGenerationGlobals.ChunkSize);
        meshFilter.mesh.bounds = new Bounds(bounds.size / 2, bounds.size);
    }

    public async Task Spawn()
    {
        state = SpawnedState.Spawning;
        blockMap = new NativeArray<uint>((bounds.size.x + 2) * (bounds.size.y + 2) * (bounds.size.z + 2), Allocator.Persistent);
        meshFilter.sharedMesh = new Mesh();
        await Generate();
        state = SpawnedState.Spawned;
    }

    private async Task Generate()
    {
        await GenerateModel();
        await GenerateMesh();
    }

    private async Task GenerateModel()
    {
        await ChunkModelGenerator.GenerateBlockmap(blockMap, bounds.position);
    }

    private async Task GenerateMesh()
    {
        await MeshGenerator.GenerateMeshData(blockMap, meshFilter.mesh);
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
    }

    public uint this[int x, int y, int z]
    {
        get
        {
            return blockMap.GetAsChunk<uint>(x, y, z);
        }
        set
        {
            blockMap.SetAsChunk<uint>(x, y, z, value);
            _ = GenerateMesh();
        }
    }
}
