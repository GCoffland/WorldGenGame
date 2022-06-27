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

public class ChunkBehavior : MonoBehaviour
{
    public static ConcurrentDictionary<Vector3Int, ChunkBehavior> All = new ConcurrentDictionary<Vector3Int, ChunkBehavior>();

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
        index = transform.position.WorldPosToChunkIndex();
        if(!All.TryAdd(index, this))
        {
            throw new InvalidOperationException("Chunk was already registered");
        }
        for(int i = 0; i < StaticDefinitions.Directions.Length; i++)
        {
            if (All.TryGetValue(index + StaticDefinitions.Directions[i], out ChunkBehavior other))
            {
                Debug.Log("Connected neighbors");
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

    public NativeArray<uint> RetrieveBorderBlocks() //  use iterators instead?
    {
        NativeArray<uint> ret = new NativeArray<uint>(WorldGenerationGlobals.ChunkSideAreas.Sum(), Allocator.Persistent);
        int index = 0;
        int n = 0;
        if (neighbors[n] != null) // x-
        {
            int x = 63;
            for (int y = 0; y < WorldGenerationGlobals.ChunkSize[1]; y++)
            {
                for (int z = 0; z < WorldGenerationGlobals.ChunkSize[2]; z++)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;

        if (neighbors[n] != null) // x+
        {
            int x = 0;
            for (int y = 0; y < WorldGenerationGlobals.ChunkSize[1]; y++)
            {
                for (int z = WorldGenerationGlobals.ChunkSize[2] - 1; z >= 0; z--)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;

        if (neighbors[n] != null) // y-
        {
            int y = 63;
            for (int z = 0; z < WorldGenerationGlobals.ChunkSize[2]; z++)
            {
                for (int x = 0; x < WorldGenerationGlobals.ChunkSize[0]; x++)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;

        if (neighbors[n] != null) // y+
        {
            int y = 0;
            for (int z = WorldGenerationGlobals.ChunkSize[2] - 1; z >= 0; z--)
            {
                for (int x = 0; x < WorldGenerationGlobals.ChunkSize[0]; x++)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;

        if (neighbors[n] != null) // z-
        {
            int z = 63;
            for (int y = 0; y < WorldGenerationGlobals.ChunkSize[1]; y++)
            {
                for (int x = WorldGenerationGlobals.ChunkSize[0] - 1; x >= 0; x--)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;

        if (neighbors[n] != null) // z+
        {
            int z = 0;
            for (int y = 0; y < WorldGenerationGlobals.ChunkSize[1]; y++)
            {
                for (int x = 0; x < WorldGenerationGlobals.ChunkSize[0]; x++)
                {
                    ret[index] = neighbors[n][x, y, z];
                    index++;
                }
            }
        }
        else
        {
            index += WorldGenerationGlobals.ChunkSideAreas[n];
        }
        n++;
        return ret;
    }

    public async Task GenerateModel()
    {
        blockMap = new NativeArray<uint>(WorldGenerationGlobals.BlockMapLength, Allocator.Persistent);
        await ModelGenerator.Singleton.GenerateBlockmap(blockMap, bounds);
    }

    public async Task GenerateMesh()
    {
        Debug.Log("Creating borderblockmap");
        NativeArray<uint> borderBlockMap = RetrieveBorderBlocks();
        Debug.Log("Passing to mesh generator");
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(blockMap, borderBlockMap);
        Debug.Log("Returning from mesh generator");
        meshFilter.mesh.Clear();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.material.mainTexture = WorldGenerationGlobals.atlas;
        borderBlockMap.Dispose();
        Debug.Log("Disposed borderblockmap");
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
