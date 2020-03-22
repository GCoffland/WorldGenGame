using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBehavior : MonoBehaviour
{
    public BoundsInt bounds;
    public GameObject chunk;
    public List<GameObject> players;
    private BoundsInt chunkBounds;
    private Hashtable activeChunks = new Hashtable();

    // Start is called before the first frame update
    void Start()
    {
        chunkBounds = chunk.GetComponent<ChunkBehavior>().bounds;
        foreach (GameObject player in players)
        {
            StartCoroutine(loadChunksAroundPlayer(player));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private const int generationRadius = 25;

    IEnumerator loadChunksAroundPlayer(GameObject player)
    {
        while (true)
        {
            Vector3Int pos = chunkCoordRound(new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, (int)player.transform.position.z));
            for(int x = pos.x - generationRadius; x < pos.x + generationRadius; x += chunkBounds.size.x)
            {
                for (int y = pos.y - generationRadius; y < pos.y + generationRadius; y += chunkBounds.size.y)
                {
                    for (int z = pos.z - generationRadius; z < pos.z + generationRadius; z += chunkBounds.size.z)
                    {
                        Vector3Int vec = new Vector3Int(x, y, z);
                        if (activeChunks[vec] == null)
                        {
                            createChunk(vec);
                        }
                    }
                }
            }
            yield return 0;
            
        }
    }
    
    Vector3Int chunkCoordRound(Vector3Int pos)
    {
        pos.x = ((int)(pos.x / chunkBounds.size.x)) * chunkBounds.size.x;
        pos.y = ((int)(pos.y / chunkBounds.size.y)) * chunkBounds.size.y;
        pos.z = ((int)(pos.z / chunkBounds.size.z)) * chunkBounds.size.z;
        return pos;
    }

    void createChunk(Vector3Int position)
    {
        activeChunks.Add(position, GameObject.Instantiate<GameObject>(chunk, position, Quaternion.identity, transform));
    }
}
