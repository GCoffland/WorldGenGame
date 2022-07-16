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
        TestGen();
    }

    private async void TestGen()
    {
        int test1 = -2;
        int test = 2;
        for (int x = test1; x < test; x++)
        {
            for (int y = test1; y < test; y++)
            {
                for (int z = test1; z < test; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    pos.Scale(WorldGenerationGlobals.ChunkSize);
                    Chunk chunk = Instantiate<Chunk>(chunkPrefab, pos, Quaternion.identity, transform);
                    await chunk.model.GenerateModel();
                }
            }
        }
        for (int x = test1; x < test; x++)
        {
            for (int y = test1; y < test; y++)
            {
                for (int z = test1; z < test; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    Chunk chunk = Chunk.All[pos];
                    await chunk.renderer.GenerateMesh();
                }
            }
        }
    }

    private void Update()
    {
        /*
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
        */
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
            chunk.renderer.enabled = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
