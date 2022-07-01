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
    private NativeArray<uint>[] borderBlockMaps = new NativeArray<uint>[6];
    private bool[] borderBlockMapsIsDirty = new bool[6];

    void Awake()
    {
        SetDefaultMesh();
        Register();
    }

    private void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
        for (int n = 0; n < 6; n++)
        {
            if (borderBlockMaps[n].IsCreated) borderBlockMaps[n].Dispose();
        }
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
        Vector3Int current = Vector3Int.zero;

        if (neighbors[n] != null) // x-
        {
            current[0] = 63;
            for (current[1] = 0; current[1] < WorldGenerationGlobals.ChunkSize[1]; current[1]++)
            {
                for (current[2] = 0; current[2] < WorldGenerationGlobals.ChunkSize[2]; current[2]++)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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
            current[0] = 0;
            for (current[1] = 0; current[1] < WorldGenerationGlobals.ChunkSize[1]; current[1]++)
            {
                for (current[2] = WorldGenerationGlobals.ChunkSize[2] - 1; current[2] >= 0; current[2]--)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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
            current[1] = 63;
            for (current[2] = 0; current[2] < WorldGenerationGlobals.ChunkSize[2]; current[2]++)
            {
                for (current[0] = 0; current[0] < WorldGenerationGlobals.ChunkSize[0]; current[0]++)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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
            current[1] = 0;
            for (current[2] = WorldGenerationGlobals.ChunkSize[2] - 1; current[2] >= 0; current[2]--)
            {
                for (current[0] = 0; current[0] < WorldGenerationGlobals.ChunkSize[0]; current[0]++)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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
            current[2] = 63;
            for (current[1] = 0; current[1] < WorldGenerationGlobals.ChunkSize[1]; current[1]++)
            {
                for (current[0] = WorldGenerationGlobals.ChunkSize[0] - 1; current[0] >= 0; current[0]--)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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
            current[2] = 0;
            for (current[1] = 0; current[1] < WorldGenerationGlobals.ChunkSize[1]; current[1]++)
            {
                for (current[0] = 0; current[0] < WorldGenerationGlobals.ChunkSize[0]; current[0]++)
                {
                    ret[index] = neighbors[n][current[0], current[1], current[2]];
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

    public async Task FetchBorderBlockmaps()
    {
        Vector3Int current = Vector3Int.zero;
        for (int n = 0; n < 6; n++)
        {
            if (!borderBlockMaps[n].IsCreated)
            {
                borderBlockMaps[n] = new NativeArray<uint>(WorldGenerationGlobals.ChunkSideAreas[n], Allocator.Persistent);
            }
            if (neighbors[n] != null)
            {
                int side_index = n / 2;
                current[side_index] = (((n + 1) % 2) * 63);
                int axis_one = (side_index + 1) % 3;
                int axis_two = (side_index + 2) % 3;
                for (current[axis_one] = 0; current[axis_one] < WorldGenerationGlobals.ChunkSize[axis_one]; current[axis_one]++)
                {
                    for (current[axis_two] = 0; current[axis_two] < WorldGenerationGlobals.ChunkSize[axis_two]; current[axis_two]++)
                    {
                        borderBlockMaps[n].SetAsChunk(current[axis_one], current[axis_two], 0, neighbors[n][current[0], current[1], current[2]]);
                    }
                }
            }
        }
    }

    public async Task GenerateMesh()
    {
        await FetchBorderBlockmaps();
        Mesh mesh = await MeshGenerator.Singleton.GenerateMeshData(blockMap, borderBlockMaps);
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
