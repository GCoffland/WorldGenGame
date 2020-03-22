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
        bounds.position = Vector3Int.RoundToInt(transform.position);
        generateChunk();
    }

    void generateChunk()
    {
        model = ChunkModelGenerator.generateSimpleGround(bounds);
        StartCoroutine(updateMeshRout());
    }

    IEnumerator updateMeshRout()
    {
        int vertexindex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for (int x = 0; x < bounds.size.x; x++) // put stuff in the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                yield return 0;
                for (int z = 0; z < bounds.size.z; z++)
                {
                    Vector3Int p = new Vector3Int(x, y, z);
                    if (model[x, y, z] != VOXELTYPE.NONE)
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            if (isVoxelSideVisible(p, VoxelData.DIRECTIONVECTORS[(DIRECTION)d], bounds))
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
        for (int x = 0; x < bounds.size.x; x++) // put stuff in the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    Vector3Int p = new Vector3Int(x, y, z);
                    if (model[x, y, z] != VOXELTYPE.NONE)
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            if (isVoxelSideVisible(p, VoxelData.DIRECTIONVECTORS[(DIRECTION)d], bounds))
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

    public bool isVoxelSideVisible(Vector3Int pos, Vector3Int dir, BoundsInt bounds)
    {
        if (model[pos.x, pos.y, pos.z] == VOXELTYPE.NONE)
        {
            return false;
        }
        else if (pos.x == 0 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.X_NEG])
        {
            return true;
        }
        else if (pos.x == bounds.size.x - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.X_POS])
        {
            return true;
        }
        else if (pos.y == 0 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Y_NEG])
        {
            return true;
        }
        else if (pos.y == bounds.size.y - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Y_POS])
        {
            return true;
        }
        else if (pos.z == 0 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Z_NEG])
        {
            return true;
        }
        else if (pos.z == bounds.size.z - 1 && dir == VoxelData.DIRECTIONVECTORS[DIRECTION.Z_POS])
        {
            return true;
        }
        else if (model[(pos.x + dir.x), (pos.y + dir.y), (pos.z + dir.z)] == VOXELTYPE.NONE)
        {
            return true;
        }
        return false;
    }

    public void setVoxel(Vector3Int pos, VOXELTYPE blocktype)
    {
        Vector3Int idexer = pos - new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        model[idexer.x, idexer.y, idexer.z] = blocktype;
        updateMesh();
    }
}
