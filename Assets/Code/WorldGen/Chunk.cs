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

public class Chunk : MonoBehaviour
{
    public static ConcurrentDictionary<Vector3Int, Chunk> All = new ConcurrentDictionary<Vector3Int, Chunk>();

    public Chunk[] neighbors = new Chunk[6];

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

    public ChunkModel model;
    new public ChunkRenderer renderer;

    private void Awake()
    {
        bounds = new BoundsInt(transform.position.ToVector3Int(), WorldGenerationGlobals.ChunkSize);
        Register();
    }

    private void OnDestroy()
    {
        UnRegister();
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
            if (All.TryGetValue(index + StaticDefinitions.Directions[i], out Chunk other))
            {
                neighbors[i] = other;
                other.neighbors[i + 1 - 2 * (i % 2)] = this;
            }
        }
    }

    private void UnRegister()
    {
        if (!All.TryRemove(index, out Chunk val))
        {
            throw new InvalidOperationException("Chunk was not registered");
        }
        for (int i = 0; i < StaticDefinitions.Directions.Length; i++)
        {
            if(neighbors[i] != null && neighbors[i].neighbors[i + 1 - 2 * (i % 2)] == this)
            {
                neighbors[i].neighbors[i + 1 - 2 * (i % 2)] = null;
            }
        }
    }
}
