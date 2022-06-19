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
using System.Linq;

public class ChunkBehavior : MonoBehaviour
{
    public static Dictionary<Vector3Int, ChunkBehavior> All = new Dictionary<Vector3Int, ChunkBehavior>();

    public ChunkBehavior[] neighbors = new ChunkBehavior[8];

    public Vector3Int index
    {
        get;
        private set;
    }

    public BoundsInt bounds
    {
        get;
        private set;
    }

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private Mesh defaultMesh;

    private NativeArray<uint> blockMap;

    void Awake()
    {
        SetDefaultMesh();
        Register();
    }

    private void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
    }

    private void Register()
    {
        index = transform.position.RoundToChunkPos();
        All.Add(index, this);
        for(int i = 0; i < StaticDefinitions.Directions.Length; i++)
        {
            if(All.TryGetValue(index + StaticDefinitions.Directions[i], out ChunkBehavior other))
            {
                neighbors[i] = other;
                other.neighbors[i + 1 - 2 * (i % 2)] = this;
            }
        }
    }

    private void UnRegister()
    {
        throw new NotImplementedException();
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

    public async Task GenerateModel()
    {
        blockMap = new NativeArray<uint>(WorldGenerationGlobals.BlockMapLength, Allocator.Persistent);
        await ModelGenerator.Singleton.GenerateBlockmap(blockMap, bounds);
    }

    public async Task GenerateMesh()
    {
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(blockMap);
        meshFilter.mesh.Clear();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
    }

    public uint this[int x, int y, int z]
    {
        get
        {
            return blockMap.GetAsChunk(x, y, z);
        }
        set
        {
            blockMap.SetAsChunk(x, y, z, value);
            _ = GenerateMesh();
        }
    }
}
