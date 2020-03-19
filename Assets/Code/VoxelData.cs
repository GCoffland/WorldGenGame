using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static readonly Vector3[] voxelVertexes = new Vector3[]
    {
        new Vector3(0,0,0), // 0
        new Vector3(0,0,1), // 1
        new Vector3(0,1,0), // 2
        new Vector3(0,1,1), // 3
        new Vector3(1,0,0), // 4
        new Vector3(1,0,1), // 5
        new Vector3(1,1,0), // 6
        new Vector3(1,1,1), // 7
    };

    public static readonly int[,] voxelFaces = new int[6, 6] // indexes corresponds to DIRECTION enum.
    {
        {5,7,1,3,1,7}, // Z+ face
        {0,2,4,6,4,2}, // Z- face
        {2,3,6,7,6,3}, // Y+ face
        {1,0,5,4,5,0}, // Y- face
        {4,6,5,7,5,6}, // X+ face
        {1,3,0,2,0,3}, // X- face
    };

    public static readonly Vector2[] voxelUVs = new Vector2[6]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1),
        new Vector2(1,0),
        new Vector2(0,1),
    };
}