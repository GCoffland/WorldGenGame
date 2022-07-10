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

public class ChunkModel : MonoBehaviour
{
    public event Action<BoundsInt> OnModelChanged;

    [SerializeField]
    private Chunk chunk;

    public NativeArray<uint> blockMap { get; private set; }

    private void OnDestroy()
    {
        if (blockMap.IsCreated) blockMap.Dispose();
    }

    public async Task GenerateModel()
    {
        Debug.Log("Generating Model for chunk: " + chunk.index);
        NativeArray<uint> tempBlockMap = new NativeArray<uint>(WorldGenerationGlobals.BlockMapLength, Allocator.Persistent);
        await ModelGenerator.Singleton.GenerateBlockmap(tempBlockMap, chunk.bounds);
        blockMap = tempBlockMap;
        OnModelChanged?.Invoke(new BoundsInt(Vector3Int.zero, chunk.bounds.size));
    }

    public void FetchBorderBlockmap(DIRECTION dir, ref NativeArray<uint> borderBlockMap)
    {
        Vector3Int current = Vector3Int.zero;
        int side_index = (int)dir / 2;
        current[side_index] = ((((int)dir + 1) % 2) * 63);
        int axis_one = (side_index + 1) % 3;
        int axis_two = (side_index + 2) % 3;
        for (current[axis_one] = 0; current[axis_one] < WorldGenerationGlobals.ChunkSize[axis_one]; current[axis_one]++)
        {
            for (current[axis_two] = 0; current[axis_two] < WorldGenerationGlobals.ChunkSize[axis_two]; current[axis_two]++)
            {
                borderBlockMap.SetAsChunk(current[axis_one], current[axis_two], 0, chunk.neighbors[(int)dir].model.GetBlock(current[0], current[1], current[2]));
            }
        }
    }

    public uint GetBlock(int x, int y, int z)
    {
        return blockMap.GetAsChunk(x, y, z);
    }

    public void SetBlock(int x, int y, int z, uint block_type)
    {
        blockMap.SetAsChunk(x, y, z, block_type);
        OnModelChanged?.Invoke(new BoundsInt(new Vector3Int(x, y, z), Vector3Int.one));
    }
}
