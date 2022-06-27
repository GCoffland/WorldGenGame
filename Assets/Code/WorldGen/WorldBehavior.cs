using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using UnityEngine.Pool;

public class WorldBehavior : MonoBehaviour
{
    public static WorldBehavior Singleton;
    public ChunkBehavior chunkPrefab;
    public GameObject[] players;

    // Start is called before the first frame update
    private void Start()
    {
        Singleton = this;
    }

    private void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int pos = players[i].transform.position.RoundToChunkPos();
            Vector3Int index = pos.WorldPosToChunkIndex();
            if (!ChunkBehavior.All.ContainsKey(index))
            {
                ChunkBehavior chunk = SpawnChunk(pos);
                _ = LoadChunk(chunk);
            }
        }
    }

    public ChunkBehavior SpawnChunk(Vector3Int pos)
    {
        ChunkBehavior chunk = Instantiate<ChunkBehavior>(chunkPrefab, pos, Quaternion.identity, transform);
        return chunk;

    }

    public async Task LoadChunk(ChunkBehavior chunk)
    {
        await chunk.GenerateModel();
        await chunk.GenerateMesh();
    }
}
