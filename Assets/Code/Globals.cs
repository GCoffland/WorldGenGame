using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals
{
    public static class BLOCK_TYPE
    {
        public static readonly int NUM_OF_BLOCK_TYPES = 2;
        public static GameObject DIRT = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Blocks/Dirt"));
        public static GameObject GRASS = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Blocks/Grass"));
    }

    public static MeshFilter[] getAllTypesOfMeshFilters()
    {
        MeshFilter[] ret = new MeshFilter[BLOCK_TYPE.NUM_OF_BLOCK_TYPES];
        ret[0] = BLOCK_TYPE.DIRT.GetComponent<MeshFilter>();
        ret[1] = BLOCK_TYPE.GRASS.GetComponent<MeshFilter>();
        return ret;
    }

    public static Material[] getAllMaterialsInOrder()
    {
        Material[] ret = new Material[BLOCK_TYPE.NUM_OF_BLOCK_TYPES];
        ret[0] = BLOCK_TYPE.DIRT.GetComponent<MeshRenderer>().material;
        ret[1] = BLOCK_TYPE.GRASS.GetComponent<MeshRenderer>().material;
        return ret;
    }

    public static void setAllBlockTypeActiveState(bool val)
    {
        BLOCK_TYPE.DIRT.SetActive(val);
        BLOCK_TYPE.GRASS.SetActive(val);
    }
}
