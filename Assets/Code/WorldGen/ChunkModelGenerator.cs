using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ChunkModelGenerator
{
    public static int seed;

    public static VOXELTYPE[,,] generateSimpleRandom(BoundsInt bounds)
    {
        VOXELTYPE[,,] model = new VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];
        UnityEngine.Random.InitState(seed);

        for (int x = bounds.xMin; x < bounds.xMax; x++) // asemble the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    float ran = UnityEngine.Random.Range(0f, 1f);
                    if (ran < 0.2f)
                    {
                        model[x, y, z] = VOXELTYPE.DIRT;
                    }
                    else
                    {
                        model[x, y, z] = VOXELTYPE.NONE;
                    }

                }
            }
        }
        return model;
    }

    public static VOXELTYPE[,,] generateSimpleGround(BoundsInt bounds)
    {
        VOXELTYPE[,,] model = new VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];
        UnityEngine.Random.InitState(seed);

        for (int x = 0; x < bounds.size.x; x++) // asemble the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    if (bounds.min.y + y == 0)
                    {
                        model[x, y, z] = VOXELTYPE.GRASS;
                    }
                    else if(bounds.min.y + y < 0)
                    {
                        model[x, y, z] = VOXELTYPE.DIRT;
                    }
                    else
                    {
                        model[x, y, z] = VOXELTYPE.NONE;
                    }

                }
            }
        }
        return model;
    }

    public static ModelData generateSimpleFunction(BoundsInt bounds)
    {
        ModelData modeldata = new ModelData(bounds);

        for (int x = 0; x < bounds.size.x; x++) // asemble the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    modeldata[x,y,z] = voxelAtPoint(new Vector3Int(x, y, z) + bounds.min);
                }
            }
        }
        return modeldata;
    }

    public static VOXELTYPE voxelAtPoint(Vector3Int v)
    {
        int temp = function(v.x, v.y, v.z);
        if (v.y == temp)
        {
            return VOXELTYPE.GRASS;
        }
        else if (v.y < temp)
        {
            return VOXELTYPE.DIRT;
        }
        else
        {
            return VOXELTYPE.NONE;
        }
    }

    private static int function(int x, int y, int z)
    {
        return (int)(21 * Mathf.Sin(x * 0.04f) + 53 * Mathf.Sin(z * 0.02f));
    }
}

public class ModelData
{
    private VOXELTYPE[,,] internalmodel;
    public VOXELTYPE this[int x, int y, int z]
    {
        get
        {
            return internalmodel[x, y, z];
        }
        set
        {
            contentCounts[value] = (int)contentCounts[value] + 1;
            contentCounts[internalmodel[x, y, z]] = (int)contentCounts[internalmodel[x, y, z]] - 1;
            internalmodel[x, y, z] = value;
        }
    }
    private Hashtable contentCounts;
    public int this[VOXELTYPE vt]
    {
        get
        {
            return (int)contentCounts[vt];
        }
    }

    public ModelData(BoundsInt bounds)
    {
        internalmodel = new VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];
        contentCounts = new Hashtable();
        foreach(VOXELTYPE t in Enum.GetValues(typeof(VOXELTYPE)))
        {
            contentCounts.Add(t, 0);
        }
    }
}