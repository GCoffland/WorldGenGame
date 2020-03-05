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
        List<List<CombineInstance>> combineInstances = new List<List<CombineInstance>>();
        for(int i = 0; i < Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES; i++)
        {
            combineInstances.Add(new List<CombineInstance>());
        }

        for (int i = chunkBounds.min.x; i < chunkBounds.max.x; i++)
        {
            for (int j = chunkBounds.min.y; j < chunkBounds.max.y; j++)
            {
                for (int k = chunkBounds.min.z; k < chunkBounds.max.z; k++)
                {
                    int num = Random.Range(0, Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES);
                    meshFilters[num].gameObject.transform.position = new Vector3(i, j, k);
                    CombineInstance temp = new CombineInstance();
                    temp.mesh = meshFilters[num].sharedMesh;
                    temp.transform = meshFilters[num].transform.localToWorldMatrix;
                    combineInstances[num].Add(temp);
                }
            }
        }
        Mesh[] combinedMeshes = new Mesh[Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES];

        for(int i = 0; i < Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES; i++)
        {
            combinedMeshes[i] = new Mesh();
            combinedMeshes[i].CombineMeshes(combineInstances[i].ToArray());
        }

        CombineInstance[] finalCombines = new CombineInstance[Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES];

        for (int i = 0; i < Globals.BLOCK_TYPE.NUM_OF_BLOCK_TYPES; i++)
        {
            finalCombines[i].mesh = combinedMeshes[i];
            finalCombines[i].transform = gameObject.transform.localToWorldMatrix;
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(finalCombines, false);
        transform.GetComponent<MeshRenderer>().materials = Globals.getAllMaterialsInOrder();
        transform.gameObject.SetActive(true);
        Globals.setAllBlockTypeActiveState(false);
    }
}
