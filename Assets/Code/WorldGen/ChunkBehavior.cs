using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BoundsInt bounds;
    public MeshCollider meshCollider;

    private ModelData modeldata;

    private List<VertexBufferStruct> Vertices;
    private List<int> Triangles;
    //private List<SubMeshDescriptor> SubMeshDescriptors;

    private bool meshNeedsApply = false;

    // Start is called before the first frame update
    void Start()
    {
        bounds.position = Vector3Int.RoundToInt(transform.position);
        Task.Run(() => {
                            try
                            {
                                modelGenerationTask();
                                voxelGenerationTask();
                            }
                            catch (System.Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                       });
    }

    void Update()
    {
        if (meshNeedsApply)
        {
            applyUpdatedMesh();
        }
    }

    void applyUpdatedMesh()
    {
        Mesh m = meshFilter.mesh;
        m.Clear();
        m.SetVertexBufferParams(Vertices.Count, VoxelData.VertexBufferLayout);
        m.SetIndexBufferParams(Triangles.Count, IndexFormat.UInt32);
        m.SetVertexBufferData(Vertices, 0, 0, Vertices.Count);
        m.SetIndexBufferData(Triangles, 0, 0, Triangles.Count);
        SubMeshDescriptor smd = new SubMeshDescriptor()
        {
            firstVertex = 0,
            vertexCount = Vertices.Count,
            baseVertex = 0,
            topology = MeshTopology.Triangles,
            indexStart = 0,
            indexCount = Triangles.Count,
        };
        m.SetSubMesh(0, smd);
        m.RecalculateBounds();
        meshFilter.mesh = m;
        meshCollider.sharedMesh = m;
        meshNeedsApply = false;
    }

    public void removeBlockAt(Vector3Int pos)
    {
        if(modeldata[pos.x, pos.y, pos.z] == VOXELTYPE.NONE)
        {
            UnityEngine.Debug.Log("Tried to remove a block that does not exist at " + pos);
        }
        else
        {
            removeVoxelAt(pos);
            modeldata[pos.x, pos.y, pos.z] = VOXELTYPE.NONE;
        }
    }

    private void removeVoxelAt(Vector3Int pos)
    {
        Stopwatch sw = new Stopwatch(); // Debug
        sw.Start(); // Debug
        VoxelBase vb = VoxelData.getVoxelType(modeldata[pos.x, pos.y, pos.z]);
        for(int d = 0; d < 6; d++)
        {
            if (isVoxelSideVisible(pos, (DIRECTION)d))
            {
                List<Vector3> l = vb.makeVoxelSideVertsAt(pos, (DIRECTION)d);
                for (int j = 0; j < Vertices.Count; j++)
                {
                    if (l.Count < Vertices.Count - j)
                    {
                        if (Vertices[j].position == l[0] && Vertices[j + 1].position == l[1] && Vertices[j + 2].position == l[2] && Vertices[j + 3].position == l[3])
                        {

                            Triangles.RemoveAll(x => x == j);
                            Triangles.RemoveAll(x => x == j + 1);
                            Triangles.RemoveAll(x => x == j + 2);
                            Triangles.RemoveAll(x => x == j + 3);
                        }
                    }
                }
            }
        }
        sw.Stop(); // Debug
        UnityEngine.Debug.Log("Removing Voxel from mesh for the chunk at " + bounds.min + " took " + sw.ElapsedMilliseconds / 1000f + " seconds"); // Debug
        applyUpdatedMesh();
    }

    public void addVoxelAt(Vector3Int pos, VOXELTYPE blocktype)
    {

    }
 
    /************Purely Task/Threaded Code************/
    static Mutex[] permittedThreads = new Mutex[8];
    Mutex chunkMutex = new Mutex();

    public static void mutexesInit()
    {
        for(int i = 0; i < permittedThreads.Length; i++)
        {
            permittedThreads[i] = new Mutex();
        }
    }

    void modelGenerationTask()
    {
        int mutexindex = WaitHandle.WaitAny(permittedThreads);
        Stopwatch sw = new Stopwatch(); // Debug
        sw.Start(); // Debug
        modeldata = ChunkModelGenerator.generateSimpleFunction(bounds);
        sw.Stop(); // Debug
        UnityEngine.Debug.Log("Generating Model for the chunk at " + bounds.min + " took " + sw.ElapsedMilliseconds / 1000f + " seconds"); // Debug
        permittedThreads[mutexindex].ReleaseMutex();
    }

    void voxelGenerationTask()
    {
        int mutexindex = WaitHandle.WaitAny(permittedThreads);
        chunkMutex.WaitOne();
        Stopwatch sw = new Stopwatch(); // Debug
        sw.Start(); // Debug
        //bool[,,] checklist = new bool[bounds.size.x, bounds.size.y, bounds.size.z];
        Vertices = new List<VertexBufferStruct>();
        Triangles = new List<int>();
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    Vector3Int p = new Vector3Int(x, y, z);
                    if (modeldata[p.x, p.y, p.z] != VOXELTYPE.NONE)
                    {
                        /*bool blockIsVisible = false;
                        int firstVertexIndex = 0;
                        int firstTriangleIndex = 0;*/
                        for (int d = 0; d < 6; d++)
                        {
                            if (isVoxelSideVisible(p, (DIRECTION)d))
                            {
                                /*if (!blockIsVisible)
                                {
                                    blockIsVisible = true;
                                    firstVertexIndex = Vertices.Count;
                                    firstTriangleIndex = Triangles.Count;
                                }*/
                                VoxelBase vb = VoxelData.getVoxelType(modeldata[p.x, p.y, p.z]);
                                vb.appendVoxelAt(p, (DIRECTION)d, ref Vertices, ref Triangles);
                            }
                        }
                        /*if (blockIsVisible)
                        {
                            SubMeshDescriptor smd = new SubMeshDescriptor();
                            smd.ba
                            SubMeshDescriptors.Add
                        }*/
                    }
                }
            }
        }
        /*Stack<Vector3Int> stack = new Stack<Vector3Int>(bounds.size.x * bounds.size.y * bounds.size.z);
        stack.Push(new Vector3Int(0, 0, 0));
        while(stack.Count > 0)
        {
            Vector3Int p = stack.Pop();
            checklist[p.x, p.y, p.z] = true;
            for (int d = 0; d < 6; d++)
            {
                Vector3Int temp = p + VoxelData.DIRECTIONVECTORS[(DIRECTION)d];
                if (isWithinBounds(temp) && !checklist[temp.x, temp.y, temp.z] && (isPerimeterBlock(temp) || modeldata[p.x, p.y, p.z] == VOXELTYPE.NONE))
                {
                    stack.Push(temp);
                }
            }
            if (modeldata[p.x, p.y, p.z] != VOXELTYPE.NONE)
            {
                for (int d = 0; d < 6; d++)
                {
                    if (isVoxelSideVisible(p, (DIRECTION)d))
                    {
                        VoxelBase vb = VoxelData.getVoxelType(modeldata[p.x, p.y, p.z]);
                        outputvertices.AddRange(vb.makeVoxelSideVertsAt(p, (DIRECTION)d).ToArray());
                        outputuvs.AddRange(vb.getVoxelUVs((DIRECTION)d).ToArray());
                        outputtriangles.AddRange(vb.getTriangles(ref vertexindex).ToArray());
                    }

                }
            }
        }*/
        meshNeedsApply = true;
        sw.Stop(); // Debug
        UnityEngine.Debug.Log("Generating Voxels for the chunk at " + bounds.min + " took " + sw.ElapsedMilliseconds/1000f + " seconds"); // Debug
        chunkMutex.ReleaseMutex();
        permittedThreads[mutexindex].ReleaseMutex();
    }
    
    /***********************End of Threaded Tasks***************************/
    private bool isWithinBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < bounds.size.x && pos.y >= 0 && pos.y < bounds.size.y && pos.z >= 0 && pos.z < bounds.size.z;
    }

    private bool isPerimeterBlock(Vector3Int pos)
    {
        return pos.x == 0 || pos.x == bounds.size.x - 1 || pos.y == 0 || pos.y == bounds.size.y - 1 || pos.z == 0 || pos.z == bounds.size.z - 1;
    }

    bool isPerimeterVoxelSide(Vector3Int pos, DIRECTION dir) // returns true if the voxel represented by the position is on the bounds of the chunk
    {
        return (pos.x == 0 && dir == DIRECTION.X_NEG) ||
               (pos.y == 0 && dir == DIRECTION.Y_NEG) || 
               (pos.z == 0 && dir == DIRECTION.Z_NEG) || 
               (pos.x == bounds.size.x - 1 && dir == DIRECTION.X_POS) || 
               (pos.y == bounds.size.y - 1 && dir == DIRECTION.Y_POS) ||
               (pos.z == bounds.size.z - 1 && dir == DIRECTION.Z_POS);
    }
    
    public bool isVoxelSideVisible(Vector3Int pos, DIRECTION dir)
    {
        if (modeldata[pos.x, pos.y, pos.z] == VOXELTYPE.NONE) // if voxel is air
        {
            return false;
        }
        else if (isPerimeterVoxelSide(pos, dir)) // if voxel is a on the chunk bounds
        {
            return ChunkModelGenerator.voxelAtPoint(bounds.min + pos + VoxelData.DIRECTIONVECTORS[dir]) == VOXELTYPE.NONE;
        }
        else if (modeldata[(pos.x + VoxelData.DIRECTIONVECTORS[dir].x), // if the side is facing air
                       (pos.y + VoxelData.DIRECTIONVECTORS[dir].y),
                       (pos.z + VoxelData.DIRECTIONVECTORS[dir].z)]
                       == VOXELTYPE.NONE)
        {
            return true;
        }
        return false;
    }
}
