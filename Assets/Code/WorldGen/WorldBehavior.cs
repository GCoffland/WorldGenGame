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
    private Dictionary<Vector3Int, ChunkBehavior> activeChunks = new Dictionary<Vector3Int, ChunkBehavior>();
    
    // Start is called before the first frame update
    private void Start()
    {
        Singleton = this;

        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int playerpos = players[i].transform.position.RoundToChunkPos();
            _ = InstantiateChunk(playerpos).Spawn();
        }

        ChunkBehavior.onAnyChunkStateChanged += onChunkFinishLoading;
    }

    private void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int playerpos = players[i].transform.position.RoundToChunkPos();
            ChunkBehavior chunk;
            if(activeChunks.TryGetValue(playerpos, out chunk))
            {
                if(chunk.state == ChunkBehavior.SpawnedState.Initialized)
                {
                    _ = chunk.Spawn();
                }
            }
            else
            {
                _ = InstantiateChunk(playerpos).Spawn();
            }
        }
    }

    private void onChunkFinishLoading(ChunkBehavior cb)
    {
        if (cb.state == ChunkBehavior.SpawnedState.Spawned)
        {
            InstantiateSurroundingChunks(cb.bounds.position);
        }
    }

    public void InstantiateSurroundingChunks(Vector3Int pos)
    {
        for(int i = 0; i < Constants.Directions.Length; i++)
        {
            Vector3Int target = pos + (Constants.Directions[i] * Constants.ChunkSize[i / 2]);
            if (!activeChunks.ContainsKey(target))
            {
                InstantiateChunk(target);
            }
        }
    }

    public ChunkBehavior InstantiateChunk(Vector3Int pos)
    {
        //Debug.Log("Instantaiting Chunk at: " + pos);
        ChunkBehavior ret = Instantiate<ChunkBehavior>(chunkPrefab, pos, Quaternion.identity, transform);
        activeChunks.Add(pos, ret);
        return ret;
    }
}
