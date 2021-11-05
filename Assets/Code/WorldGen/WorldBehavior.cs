using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;

public class WorldBehavior : MonoBehaviour
{
    public static WorldBehavior instance;
    public GameObject chunkPrefab;
    public List<GameObject> players;
    private Hashtable activeChunks = new Hashtable();
    public Dictionary<Vector3Int, VOXELTYPE> blockChangeLog = new Dictionary<Vector3Int, VOXELTYPE>();
    public bool chunkCurrentlyGenerating = false;
    private Queue<Vector3Int> chunkGenQueue = new Queue<Vector3Int>();



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        generationRadius = Constants.ChunkSize.x * generationRadius;
        generationHeight = Constants.ChunkSize.x * generationHeight;
        foreach (GameObject player in players)
        {
            StartCoroutine(loadChunksAroundPlayer(player));
        }
    }

    private void Update()
    {
        if(chunkGenQueue.Count > 0 && !chunkCurrentlyGenerating)
        {
            SpawnChunk(new BoundsInt(chunkGenQueue.Dequeue(), Constants.ChunkSize));
        }
    }

    void queueChunk(Vector3Int position)
    {
        chunkGenQueue.Enqueue(position);
    }

    public GameObject SpawnChunk(BoundsInt bounds)
    {
        //Debug.Log("Spawning Chunk at: " + bounds.position);
        chunkCurrentlyGenerating = true;
        GameObject ret = Instantiate(chunkPrefab, bounds.position, Quaternion.identity, transform);
        activeChunks.Add(bounds.position, ret);
        //_ = ret.GetComponent<ChunkBehavior>().Spawn();
        return ret;
    }

    private int generationRadius = 2;
    private int generationHeight = 3;

    IEnumerator loadChunksAroundPlayer(GameObject player)
    {
        while (true)
        {
            Vector3Int playerpos = player.transform.position.RoundToChunkChunkPos();
            for (int x = playerpos.x - generationRadius; x < playerpos.x + generationRadius; x += Constants.ChunkSize.x)
            {
                for (int y = playerpos.y - generationHeight; y < playerpos.y + generationHeight; y += Constants.ChunkSize.y)
                {
                    for (int z = playerpos.z - generationRadius; z < playerpos.z + generationRadius; z += Constants.ChunkSize.z)
                    {
                        Vector3Int temp = new Vector3Int(x, y, z);
                        if (!activeChunks.Contains(temp) && !chunkGenQueue.Contains(temp))
                        {
                            queueChunk(temp);
                        }
                    }
                }
            }
            yield return 0;
        }
    }

    public void removeBlockAt(Vector3Int pos)
    {
        GameObject go = (GameObject)activeChunks[pos.RoundToChunkChunkPos()];
        if (go == null)
        {
            Debug.Log("Null GameObject in activeChunks HashTable at Vector: " + pos.RoundToChunkChunkPos());
        }
        else
        {
            Debug.Log("Trying to remove Voxel from pos: " + pos);
            ChunkBehavior cb = go.GetComponent<ChunkBehavior>();
            cb.removeBlockAt(pos - Vector3Int.FloorToInt(go.transform.position));
        }
    }
}
