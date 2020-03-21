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

    public static bool isVoxelSideVisible(Vector3 pos, Vector3 dir, VOXELTYPE[,,] model, BoundsInt bounds)
    {
        if (model[(int)pos.x, (int)pos.y, (int)pos.z] == VOXELTYPE.NONE)
        {
            return false;
        }
        else if (pos.x == bounds.xMin && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.X_NEG])
        {
            return true;
        }
        else if (pos.x == bounds.xMax - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.X_POS])
        {
            return true;
        }
        else if (pos.y == bounds.yMin && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Y_NEG])
        {
            return true;
        }
        else if (pos.y == bounds.yMax - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Y_POS])
        {
            return true;
        }
        else if (pos.z == bounds.zMin && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Z_NEG])
        {
            return true;
        }
        else if (pos.z == bounds.zMax - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Z_POS])
        {
            return true;
        }
        else if (model[(int)(pos.x + dir.x), (int)(pos.y + dir.y), (int)(pos.z + dir.z)] == VOXELTYPE.NONE)
        {
            return true;
        }
        return false;
    }
}
