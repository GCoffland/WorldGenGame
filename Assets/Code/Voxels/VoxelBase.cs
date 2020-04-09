using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelBase
{
    public readonly static VoxelBase instance = new VoxelBase();
    virtual protected Vector2[] textureOrigins { get; } = new Vector2[]
    {
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
        new Vector2(3 * VoxelData.TNF, 3 * VoxelData.TNF),
    };
    virtual protected Vector3[] voxelVertexes { get; } = new Vector3[]
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
    virtual protected int[,] voxelFaces { get; } = new int[6, 4] // indexes corresponds to DIRECTION enum.
    {
        {5,7,1,3}, // Z+ face
        {0,2,4,6}, // Z- face
        {2,3,6,7}, // Y+ face
        {1,0,5,4}, // Y- face
        {4,6,5,7}, // X+ face
        {1,3,0,2}, // X- face
    };
    virtual protected Vector2[] voxelUVs { get; } = new Vector2[4]
    {
        new Vector2(0,0), //sides
        new Vector2(0, VoxelData.TNF),
        new Vector2(VoxelData.TNF, 0),
        new Vector2(VoxelData.TNF, VoxelData.TNF),
    };
    protected VoxelBase(){} // private constructor

    public virtual void appendVoxelAt(Vector3 pos, DIRECTION dir, ref List<VertexBufferStruct> verticies, ref List<int> triangles)
    {
        int startindex = verticies.Count;
        for(int i = 0; i < voxelFaces.GetLength(1); i++)
        {
            VertexBufferStruct temp = new VertexBufferStruct();
            temp.position = pos + voxelVertexes[voxelFaces[(int)dir, i]];
            temp.TexCoord = textureOrigins[(int)dir] + instance.voxelUVs[i];
            temp.normal = VoxelData.DIRECTIONVECTORS[dir];
            verticies.Add(temp);
        }
        triangles.Add(startindex); // get an array of all the indexes of all the voxels added in the vertices list in order (triangle order)
        triangles.Add(startindex + 1);
        triangles.Add(startindex + 2);
        triangles.Add(startindex + 3);
        triangles.Add(startindex + 2);
        triangles.Add(startindex + 1);
    }

    public virtual List<Vector3> makeVoxelSideVertsAt(Vector3 pos, DIRECTION dir)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 0]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 1]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 2]]);
        vertices.Add(pos + instance.voxelVertexes[instance.voxelFaces[(int)dir, 3]]);
        return vertices;
    }

    public virtual List<Vector2> getVoxelUVs(DIRECTION dir)
    {
        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(textureOrigins[(int)dir] + instance.voxelUVs[0]);
        uvs.Add(textureOrigins[(int)dir] + instance.voxelUVs[1]);
        uvs.Add(textureOrigins[(int)dir] + instance.voxelUVs[2]);
        uvs.Add(textureOrigins[(int)dir] + instance.voxelUVs[3]);
        return uvs;
    }

    public virtual List<int> getTriangles(ref int vertexindex) // takes in vertex index, because if you are doing multiple blocks, you need to share vertex indexes accross all calls.
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
