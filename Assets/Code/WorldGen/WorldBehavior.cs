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

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        ChunkBehavior.mutexesInit();
        chunkBounds = chunk.GetComponent<ChunkBehavior>().bounds;
        foreach (GameObject player in players)
        {
            StartCoroutine(loadChunksAroundPlayer(player));
        }
    }

    private readonly int generationRadius = 1;
    private readonly int generationHeight = 1;

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

    private int roundToNearestMultiple(float input, int multiple)
    {
        if(input%multiple >= (float)multiple / 2)
        {
            return (((int)(input / multiple)) + 1) * multiple;
        }
        else
        {
            return ((int)(input / multiple)) * multiple;
        }
    }

    private Vector3Int roundToNearestMultiple(Vector3 input, Vector3Int multiple)
    {
        return new Vector3Int(roundToNearestMultiple(input.x,multiple.x),
                              roundToNearestMultiple(input.y, multiple.y),
                              roundToNearestMultiple(input.z, multiple.z));
    }

    void createChunk(Vector3Int position)
    {
        activeChunks.Add(position, GameObject.Instantiate<GameObject>(chunk, position, Quaternion.identity, transform));
    }

    public GameObject getChunkAt(Vector3Int pos)
    {
        return (GameObject)activeChunks[pos];
    }
}
