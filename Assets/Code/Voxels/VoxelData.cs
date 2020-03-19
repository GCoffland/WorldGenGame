using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class VoxelData
{
    public enum DIRECTION{
        Z_POS,
        Z_NEG,
        Y_POS,
        Y_NEG,
        X_POS,
        X_NEG,
    };

    public static Dictionary<DIRECTION, Vector3> DIRECTIONVECTORS = new Dictionary<DIRECTION, Vector3>()
    {
        {DIRECTION.Z_POS, new Vector3(0,0,1)},
        {DIRECTION.Z_NEG, new Vector3(0,0,-1)},
        {DIRECTION.Y_POS, new Vector3(0,1,0)},
        {DIRECTION.Y_NEG, new Vector3(0,-1,0)},
        {DIRECTION.X_POS, new Vector3(1,0,0)},
        {DIRECTION.X_NEG, new Vector3(-1,0,0)},
    };

    public enum VOXELTYPE
    {
        NONE,
        DIRT,
    };

    public static Dictionary<VOXELTYPE, Type> VOXELS = new Dictionary<VOXELTYPE, Type>()
    {
        {VOXELTYPE.DIRT, typeof(DirtVoxel)},
    };
}