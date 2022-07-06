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
    public Chunk chunkPrefab;
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
            if (!Chunk.All.ContainsKey(index))
            {
                Chunk chunk = SpawnChunk(pos);
                _ = LoadChunk(chunk);
            }
        }
    }

    public Chunk SpawnChunk(Vector3Int pos)
    {
        Chunk chunk = Instantiate<Chunk>(chunkPrefab, pos, Quaternion.identity, transform);
        return chunk;

    }

    public async Task LoadChunk(Chunk chunk)
    {
        Debug.Log("Loading Chunk: " + chunk.index);
        try
        {
            await chunk.model.GenerateModel();
            await chunk.renderer.GenerateMesh();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
