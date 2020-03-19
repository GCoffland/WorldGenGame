using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BoundsInt bounds;

    // Start is called before the first frame update
    void Start()
    {
        int vertexindex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        VoxelData.VOXELTYPE[,,] model = makeModel();

        for (int x = bounds.xMin; x < bounds.xMax; x++) // put stuff in the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    Vector3 p = new Vector3(x, y, z);
                    for(int d = 0; d < 6; d++)
                    {
                        if(isVoxelSideVisible(p, VoxelData.DIRECTIONVECTORS[(VoxelData.DIRECTION)d], model))
                        {
                            vertices.AddRange(VoxelBase.makeVoxelSideVertsAt(p, (VoxelData.DIRECTION)d).ToArray());
                            uvs.AddRange(VoxelBase.getVoxelUVs().ToArray());
                            triangles.AddRange(VoxelBase.getTriangles(ref vertexindex).ToArray());
                        }
                    }
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    public VoxelData.VOXELTYPE[,,] makeModel()
    {
        VoxelData.VOXELTYPE[,,] model = new VoxelData.VOXELTYPE[bounds.size.x, bounds.size.y, bounds.size.z];

        for (int x = bounds.xMin; x < bounds.xMax; x++) // asemble the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    float ran = Random.Range(0f, 1f);
                    if (ran < 0.2f)
                    {
                        model[x, y, z] = VoxelData.VOXELTYPE.DIRT;
                    }
                    else
                    {
                        model[x, y, z] = VoxelData.VOXELTYPE.NONE;
                    }
                        
                }
            }
        }
        return model;
    }

    public bool isVoxelSideVisible(Vector3 pos, Vector3 dir, VoxelData.VOXELTYPE[,,] model)
    {
        if (pos.x == bounds.xMin && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.X_NEG])
        {
            return true;
        }else if(pos.x == bounds.xMax - 1 && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.X_POS])
        {
            return true;
        }
        else if (pos.y == bounds.yMin && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.Y_NEG])
        {
            return true;
        }
        else if (pos.y == bounds.yMax - 1 && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.Y_POS])
        {
            return true;
        }
        else if (pos.z == bounds.zMin && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.Z_NEG])
        {
            return true;
        }
        else if (pos.z == bounds.zMax - 1 && dir == VoxelData.DIRECTIONVECTORS[VoxelData.DIRECTION.Z_POS])
        {
            return true;
        }
        return false;
    }
}
