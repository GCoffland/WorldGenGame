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

    public ChunkBehavior[] neighbors = new ChunkBehavior[6];

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
    private bool[] borderBlockMapIsClean = new bool[6];

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
                other.borderBlockMapIsClean[i + 1 - 2 * (i % 2)] = false;
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

    private bool IsBorderBlock(Vector3Int pos, out DIRECTION dir_index)
    {
        for (int i = 0; i < 3; i++)
        {
            if (pos[i] <= 0)
            {
                dir_index = (DIRECTION)((int)i * 2);
                return true;
            }
            else if (pos[i] >= WorldGenerationGlobals.ChunkSize[i] - 1)
            {
                dir_index = (DIRECTION)((int)i * 2) + 1;
                return true;
            }
        }
        dir_index = (DIRECTION)(-1);
        return false;
    }

    public async Task GenerateModel()
    {
        blockMap = new NativeArray<uint>(WorldGenerationGlobals.BlockMapLength, Allocator.Persistent);
        await ModelGenerator.Singleton.GenerateBlockmap(blockMap, bounds);
    }

    public void FetchBorderBlockmap(DIRECTION dir)
    {
        Vector3Int current = Vector3Int.zero;
        if (!borderBlockMaps[(int)dir].IsCreated)
        {
            borderBlockMaps[(int)dir] = new NativeArray<uint>(WorldGenerationGlobals.ChunkSideAreas[(int)dir], Allocator.Persistent);
        }
        if (neighbors[(int)dir] != null)
        {
            int side_index = (int)dir / 2;
            current[side_index] = ((((int)dir + 1) % 2) * 63);
            int axis_one = (side_index + 1) % 3;
            int axis_two = (side_index + 2) % 3;
            for (current[axis_one] = 0; current[axis_one] < WorldGenerationGlobals.ChunkSize[axis_one]; current[axis_one]++)
            {
                for (current[axis_two] = 0; current[axis_two] < WorldGenerationGlobals.ChunkSize[axis_two]; current[axis_two]++)
                {
                    borderBlockMaps[(int)dir].SetAsChunk(current[axis_one], current[axis_two], 0, neighbors[(int)dir][current[0], current[1], current[2]]);
                }
            }
        }
    }

    public void TryFetchBorderBlockmaps()
    {
        for (int n = 0; n < 6; n++)
        {
            if (!borderBlockMapIsClean[n])
            {
                FetchBorderBlockmap((DIRECTION)n);
                borderBlockMapIsClean[n] = true;
            }
        }
    }

    public async Task GenerateMesh()
    {
        TryFetchBorderBlockmaps();
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
            if(IsBorderBlock(new Vector3Int(x, y, z), out DIRECTION dir))
            {
                neighbors[(int)dir].borderBlockMapIsClean[(int)dir + 1 - 2 * ((int)dir % 2)] = false;
            }
        }
    }
}
