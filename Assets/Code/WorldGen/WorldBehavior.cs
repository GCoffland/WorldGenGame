using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;
using System.Threading.Tasks;
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

        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int playerpos = players[i].transform.position.RoundToChunkPos();
            _ = SpawnChunk(playerpos);
        }
    }

    private void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int playerpos = players[i].transform.position.RoundToChunkPos();
            if (!ChunkBehavior.All.ContainsKey(playerpos))
            {
                _ = SpawnChunk(playerpos);
            }
        }
    }

    public async Task<ChunkBehavior> SpawnChunk(Vector3Int pos)
    {
        ChunkBehavior ret = Instantiate<ChunkBehavior>(chunkPrefab, pos, Quaternion.identity, transform);
        await ret.GenerateModel();
        await ret.GenerateMesh();
        return ret;
    }
}
