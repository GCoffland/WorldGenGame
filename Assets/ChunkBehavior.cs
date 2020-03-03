using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehavior : MonoBehaviour
{
    public BoundsInt chunkBounds;
    public string blockPath;
    private GameObject block;

    // Start is called before the first frame update
    void Start()
    {
        Resources.Load<GameObject>(blockPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateChunk()
    {
        for (int i = chunkBounds.min.x; i < chunkBounds.max.x; i++)
        {
            for (int j = chunkBounds.min.y; j < chunkBounds.max.y; j++)
            {
                for (int k = chunkBounds.min.z; k < chunkBounds.max.z; k++)
                {
                    Instantiate<GameObject>(block, new Vector3(i, j, k), Quaternion.identity);
                }
            }
        }
    }
}
