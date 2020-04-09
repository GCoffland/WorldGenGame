using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ChunkModelGenerator
{
    public static int seed;

    public static ModelData generateSimpleRandom(BoundsInt bounds)
    {
        ModelData modeldata = new ModelData(bounds);
        System.Random ran = new System.Random();
        for (int x = 0; x < bounds.size.x; x++) // asemble the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    if (ran.NextDouble() < 0.2)
                    {
                        modeldata[x, y, z] = VOXELTYPE.DIRT;
                    }
                    else
                    {
                        modeldata[x, y, z] = VOXELTYPE.NONE;
                    }

                }
            }
        }
        return modeldata;
    }

    public static ModelData generateBasicWorstCase(BoundsInt bounds)
    {
        ModelData modeldata = new ModelData(bounds);
        for (int x = 0; x < bounds.size.x; x++) // asemble the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    if ((x+y+z) % 2 == 0)
                    {
                        modeldata[x, y, z] = VOXELTYPE.DIRT;
                    }
                    else
                    {
                        modeldata[x, y, z] = VOXELTYPE.NONE;
                    }

                }
            }
        }
        return modeldata;
    }

    public static ModelData generateSimpleGround(BoundsInt bounds)
    {
        ModelData model = new ModelData(bounds);
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
        return (int)(0.6 * x * x + 0.5 * z * z - z * x * 0.2);
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