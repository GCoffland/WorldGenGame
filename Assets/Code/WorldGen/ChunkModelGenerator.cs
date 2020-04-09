using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class ChunkModelGenerator
{
    public static int seed;
    private static Mutex generationMutex = new Mutex();

    public static VOXELTYPE voxelAtPoint(Vector3Int v)
    {
        VOXELTYPE ret;
        if (WorldBehavior.instance.blockChangeLog.TryGetValue(v, out ret))
        {
            return ret;
        }
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