using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkModelGenerator
{
    public static int seed;

    public static VOXELTYPE[,,] generateSimpleRandom(BoundsInt bounds)
    {
        VOXELTYPE[,,] model = new VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];
        Random.InitState(seed);

        for (int x = bounds.xMin; x < bounds.xMax; x++) // asemble the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    float ran = Random.Range(0f, 1f);
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
        Random.InitState(seed);

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

    public static VOXELTYPE[,,] generateSimpleFunction(BoundsInt bounds)
    {
        VOXELTYPE[,,] model = new VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];
        Random.InitState(seed);

        for (int x = 0; x < bounds.size.x; x++) // asemble the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    int temp = function(bounds.min.x + x, bounds.min.y + y, bounds.min.z + z);
                    if (bounds.min.y + y == temp)
                    {
                        model[x, y, z] = VOXELTYPE.GRASS;
                    }
                    else if (bounds.min.y + y < temp)
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

    private static int function(int x, int y, int z)
    {
        return (int)(82 * Mathf.Sin(x * 0.01f) + 203 * Mathf.Sin(z * 0.005f));
    }
}
