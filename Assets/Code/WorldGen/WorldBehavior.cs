using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;
using System.Threading.Tasks;
using UnityEngine.Pool;

public class WorldBehavior : MonoBehaviour
{
    public static WorldBehavior Singleton;
    public GameObject chunkPrefab;
    public GameObject[] players;
    private Hashtable activeChunks = new Hashtable();

    // Start is called before the first frame update
    private void Start()
    {
        Singleton = this;

        for (int i = 0; i < players.Length; i++)
        {
            Vector3Int playerpos = players[i].transform.position.RoundToChunkChunkPos();
            InstantiateChunk(playerpos);
        }

        ChunkBehavior.onAnyChunkStateChanged += (ChunkBehavior cb) =>
        {
            if(cb.state == ChunkBehavior.SpawnedState.Spawned)
            {
                InstantiateSurroundingChunks(cb.bounds.position);
            }
        };
    }

    public void InstantiateSurroundingChunks(Vector3Int pos)
    {
        for(int i = 0; i < Constants.Directions.Length; i++)
        {
            Vector3Int target = pos + (Constants.Directions[i] * Constants.ChunkSize[i / 2]);
            if (!activeChunks.Contains(target))
            {
                InstantiateChunk(target);
            }
        }
    }

    public GameObject InstantiateChunk(Vector3Int pos)
    {
        Debug.Log("Instantaiting Chunk at: " + pos);
        GameObject ret = Instantiate(chunkPrefab, pos, Quaternion.identity, transform);
        activeChunks.Add(pos, ret);
        return ret;
    }
}
