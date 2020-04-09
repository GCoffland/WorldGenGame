using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtVoxel : VoxelBase
{
    new public readonly static DirtVoxel instance = new DirtVoxel();

    protected override Vector2[] textureOrigins { get; } = new Vector2[]
    {
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
        new Vector2(2 * VoxelData.TNF,0),
    };

    protected DirtVoxel() { } // private constructor
}
