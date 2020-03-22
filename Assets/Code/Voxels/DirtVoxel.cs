using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtVoxel : VoxelBase
{
    new public readonly static DirtVoxel instance = new DirtVoxel();

    protected override Hashtable textureOrigins { get; } = new Hashtable()
    {
        {DIRECTION.X_NEG, new Vector2(2 * VoxelData.TNF,0)},
        {DIRECTION.X_POS, new Vector2(2 * VoxelData.TNF,0)},
        {DIRECTION.Y_NEG, new Vector2(2 * VoxelData.TNF,0)},
        {DIRECTION.Y_POS, new Vector2(2 * VoxelData.TNF,0)},
        {DIRECTION.Z_NEG, new Vector2(2 * VoxelData.TNF,0)},
        {DIRECTION.Z_POS, new Vector2(2 * VoxelData.TNF,0)},
    };

    protected DirtVoxel() { } // private constructor
}
