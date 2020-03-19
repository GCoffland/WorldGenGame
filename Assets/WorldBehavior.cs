using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBehavior : MonoBehaviour
{
    public BoundsInt bounds;
    public GameObject chunk;

    // Start is called before the first frame update
    void Start()
    {
        BoundsInt chunkbounds = chunk.GetComponent<ChunkBehavior>().bounds;
        for (int x = bounds.xMin; x < bounds.xMax; x += chunkbounds.size.x)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y += chunkbounds.size.y)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z += chunkbounds.size.z)
                {
                    GameObject.Instantiate<GameObject>(chunk, new Vector3(x,y,z), Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
