using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BoundsInt bounds;
    public MeshCollider meshCollider;

    private VOXELTYPE[,,] model;

    // Start is called before the first frame update

    void Start()
    {
        generateChunk();
    }

    void generateChunk()
    {
        model = ChunkModelGenerator.generateSimpleRandom(bounds);
        StartCoroutine(updateMeshRout());
    }

    IEnumerator updateMeshRout()
    {
        int vertexindex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) // put stuff in the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                yield return 0;
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    Vector3 p = new Vector3(x, y, z);
                    if (model[x, y, z] != VOXELTYPE.NONE)
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            if (ChunkModelGenerator.isVoxelSideVisible(p, VoxelData.DIRECTIONVECTORS[(DIRECTION)d], model, bounds))
                            {
                                System.Reflection.FieldInfo fieldinfo = VoxelData.VoxelTypes[model[x, y, z]].GetField("instance");
                                VoxelBase vb = (VoxelBase)fieldinfo.GetValue(fieldinfo);
                                vertices.AddRange(vb.makeVoxelSideVertsAt(p, (DIRECTION)d).ToArray());
                                uvs.AddRange(vb.getVoxelUVs((DIRECTION)d).ToArray());
                                triangles.AddRange(vb.getTriangles(ref vertexindex).ToArray());
                            }
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void updateMesh()
    {
        int vertexindex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) // put stuff in the model
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int z = bounds.zMin; z < bounds.zMax; z++)
                {
                    Vector3 p = new Vector3(x, y, z);
                    if (model[x, y, z] != VOXELTYPE.NONE)
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            if (ChunkModelGenerator.isVoxelSideVisible(p, VoxelData.DIRECTIONVECTORS[(DIRECTION)d], model, bounds))
                            {
                                System.Reflection.FieldInfo fieldinfo = VoxelData.VoxelTypes[model[x, y, z]].GetField("instance");
                                VoxelBase vb = (VoxelBase)fieldinfo.GetValue(fieldinfo);
                                vertices.AddRange(vb.makeVoxelSideVertsAt(p, (DIRECTION)d).ToArray());
                                uvs.AddRange(vb.getVoxelUVs((DIRECTION)d).ToArray());
                                triangles.AddRange(vb.getTriangles(ref vertexindex).ToArray());
                            }
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void setVoxel(Vector3Int pos, VOXELTYPE blocktype)
    {
        Vector3Int idexer = pos - new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        model[idexer.x, idexer.y, idexer.z] = blocktype;
        updateMesh();
    }
}
