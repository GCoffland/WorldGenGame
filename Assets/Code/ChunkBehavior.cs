using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehavior : MonoBehaviour
{
    public BoundsInt chunkBounds;

    // Start is called before the first frame update
    void Start()
    {
        GenerateChunk();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateChunk()
    {
        Globals.setAllBlockTypeActiveState(true);
        MeshFilter[] meshFilters = Globals.getAllTypesOfMeshFilters();
        List<CombineInstance>[] combineInstances = new List<CombineInstance>[Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES];

        for (int i = chunkBounds.min.x; i < chunkBounds.max.x; i++)
        {
            for (int j = chunkBounds.min.y; j < chunkBounds.max.y; j++)
            {
                for (int k = chunkBounds.min.z; k < chunkBounds.max.z; k++)
                {
                    int num = Random.Range(0, Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES);
                    meshFilters[num].gameObject.transform.position = new Vector3(i, j, k);
                    //combineInstances[num].Add(new CombineInstance());
                    //combineInstances[num][combineInstances[num].Count-1].mesh = meshFilters[num].sharedMesh;
                    //combine[(i * chunkBounds.size.y * chunkBounds.size.z) + (j * chunkBounds.size.y) + k].mesh = meshFilters[num].sharedMesh;
                    //combine[(i * chunkBounds.size.y * chunkBounds.size.z) + (j * chunkBounds.size.y) + k].transform = meshFilters[num].transform.localToWorldMatrix;
                }
            }
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        //transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
        Globals.setAllBlockTypeActiveState(false);
    }
}
