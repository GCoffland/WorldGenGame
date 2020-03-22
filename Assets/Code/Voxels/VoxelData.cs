﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




// GLOBAL ENUMS
public enum DIRECTION
{
    Z_POS,
    Z_NEG,
    Y_POS,
    Y_NEG,
    X_POS,
    X_NEG,
};

public enum VOXELTYPE
{
    NONE,
    DEBUG,
    DIRT,
    GRASS,
};

public static class VoxelData
{
    public const float TNF = 0.25f; // Texture Normalization Factor (1 / the number of textures in the x direction)

    public static Dictionary<DIRECTION, Vector3Int> DIRECTIONVECTORS = new Dictionary<DIRECTION, Vector3Int>()
    {
        {DIRECTION.Z_POS, new Vector3Int(0,0,1)},
        {DIRECTION.Z_NEG, new Vector3Int(0,0,-1)},
        {DIRECTION.Y_POS, new Vector3Int(0,1,0)},
        {DIRECTION.Y_NEG, new Vector3Int(0,-1,0)},
        {DIRECTION.X_POS, new Vector3Int(1,0,0)},
        {DIRECTION.X_NEG, new Vector3Int(-1,0,0)},
    };

    public static Dictionary<VOXELTYPE, Type> VoxelTypes = new Dictionary<VOXELTYPE, Type>()
    {
        {VOXELTYPE.NONE, null},
        {VOXELTYPE.DEBUG, typeof(VoxelBase)},
        {VOXELTYPE.DIRT, typeof(DirtVoxel)},
        {VOXELTYPE.GRASS, typeof(GrassVoxel)},
    };

    public static VoxelBase getVoxelType(VOXELTYPE t)
    {
        switch (t)
        {
            case (VOXELTYPE.DEBUG):
                return VoxelBase.instance;
            case (VOXELTYPE.DIRT):
                return DirtVoxel.instance;
            case (VOXELTYPE.GRASS):
                return GrassVoxel.instance;
            default:
                return null;
        }
    }
}