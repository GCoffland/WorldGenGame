using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassVoxel : VoxelBase
{
    new public readonly static GrassVoxel instance = new GrassVoxel();

    protected override Vector2[] textureOrigins { get; } = new Vector2[]
    {
        new Vector2(0,0),
        new Vector2(0,0),
        new Vector2(VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(0,0),
        new Vector2(0,0),
    };

    protected GrassVoxel() { } // private constructor
}
