using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelBase
{
    protected static VoxelBase instance = new VoxelBase();
    protected readonly Vector3[] voxelVertexes = new Vector3[]
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
    protected readonly int[,] voxelFaces = new int[6, 4] // indexes corresponds to DIRECTION enum.
    {
        {5,7,1,3}, // Z+ face
        {0,2,4,6}, // Z- face
        {2,3,6,7}, // Y+ face
        {1,0,5,4}, // Y- face
        {4,6,5,7}, // X+ face
        {1,3,0,2}, // X- face
    };
    protected readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1),
    };

    protected VoxelBase(){} // private constructor

    public static List<Vector3> makeVoxelSideVertsAt(Vector3 pos, VoxelData.DIRECTION dir)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 0]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 1]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 2]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 3]]);
        return vertices;
    }

    public static List<Vector2> getVoxelUVs()
    {
        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(instance.voxelUVs[0]);
        uvs.Add(instance.voxelUVs[1]);
        uvs.Add(instance.voxelUVs[2]);
        uvs.Add(instance.voxelUVs[3]);
        return uvs;
    }

    public static List<int> getTriangles(ref int vertexindex) // takes in vertex index, because if you are doing multiple blocks, you need to share vertex indexes accross all calls.
    {
        List<int> triangles = new List<int>();
        triangles.Add(vertexindex); // get an array of all the indexes of all the voxels added in the vertices list in order (triangle order)
        triangles.Add(vertexindex + 1);
        triangles.Add(vertexindex + 2);
        triangles.Add(vertexindex + 3);
        triangles.Add(vertexindex + 2);
        triangles.Add(vertexindex + 1);
        vertexindex += 4;
        return triangles;
    }
}
