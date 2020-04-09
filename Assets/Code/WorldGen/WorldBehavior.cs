using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBehavior : MonoBehaviour
{
    public static WorldBehavior instance;
    public BoundsInt bounds;
    public GameObject chunk;
    public List<GameObject> players;
    private BoundsInt chunkBounds;
    private Hashtable activeChunks = new Hashtable();
    public Dictionary<Vector3Int, VOXELTYPE> blockChangeLog = new Dictionary<Vector3Int, VOXELTYPE>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        ChunkBehavior.mutexesInit();
        chunkBounds = chunk.GetComponent<ChunkBehavior>().bounds;
        generationRadius = chunkBounds.size.x * generationRadius;
        generationHeight = chunkBounds.size.x * generationHeight;
        foreach (GameObject player in players)
        {
            StartCoroutine(loadChunksAroundPlayer(player));
        }
    }

    void createChunk(Vector3Int position)
    {
        Debug.Log("Instantiating Chunk at: " + position);
        activeChunks.Add(position, GameObject.Instantiate<GameObject>(chunk, position, Quaternion.identity, transform));
    }

    private int generationRadius = 1;
    private int generationHeight = 1;

    IEnumerator loadChunksAroundPlayer(GameObject player)
    {
        while (true)
        {
            Vector3Int playerpos = worldPosToChunkPos(player.transform.position);
            for (int x = playerpos.x - generationRadius; x < playerpos.x + generationRadius; x += chunkBounds.size.x)
            {
                for (int y = playerpos.y - generationHeight; y < playerpos.y + generationHeight; y += chunkBounds.size.y)
                {
                    for (int z = playerpos.z - generationRadius; z < playerpos.z + generationRadius; z += chunkBounds.size.z)
                    {
                        Vector3Int temp = new Vector3Int(x, y, z);
                        if (activeChunks[temp] == null)
                        {
                            createChunk(temp);
                        }
                    }
                }
            }
            yield return 0;
        }
    }

    public Vector3Int worldPosToChunkPos(Vector3 input)
    {
        return new Vector3Int(Mathf.FloorToInt(input.x / chunkBounds.size.x) * chunkBounds.size.x,
                              Mathf.FloorToInt(input.y / chunkBounds.size.y) * chunkBounds.size.y,
                              Mathf.FloorToInt(input.z / chunkBounds.size.z) * chunkBounds.size.z);
    }

    public void removeBlockAt(Vector3Int pos)
    {
        GameObject go = (GameObject)activeChunks[worldPosToChunkPos(pos)];
        if (go == null)
        {
            Debug.Log("Null GameObject in activeChunks HashTable at Vector: " + worldPosToChunkPos(pos));
        }
        else
        {
            Debug.Log("Trying to remove Voxel from pos: " + pos);
            ChunkBehavior cb = go.GetComponent<ChunkBehavior>();
            cb.removeBlockAt(pos - Vector3Int.FloorToInt(go.transform.position));
        }
    }
}
