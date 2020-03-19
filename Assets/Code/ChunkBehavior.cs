using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    // Start is called before the first frame update
    void Start()
    {
        int vertexindex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for(int j = 0; j < 6; j++)
        {
            for (int i = 0; i < 6; i++)
            {
                int triangleindex = VoxelData.voxelFaces[j, i];
                vertices.Add(VoxelData.voxelVertexes[triangleindex]);
                triangles.Add(vertexindex);
                uvs.Add(VoxelData.voxelUVs[i]);
                vertexindex++;
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    
}
