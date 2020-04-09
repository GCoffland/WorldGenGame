using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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

public struct VertexBufferStruct
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 TexCoord;
}

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

    public readonly static VertexAttributeDescriptor[] VertexBufferLayout = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
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