using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BoundsInt bounds;
    public MeshCollider meshCollider;

    private ModelData modeldata;

    // Start is called before the first frame update
    void Start()
    {
        bounds.position = Vector3Int.RoundToInt(transform.position);
        currentTask = Task.Run(() =>    {
                                            try
                                            {
                                                modelGenerationTask();
                                                voxelGenerationTask();
                                            }
                                            catch (System.Exception e)
                                            {
                                                Debug.LogException(e);
                                            }
                                        });
    }

    void Update()
    {
        if (meshNeedsApply)
        {
            StartCoroutine(applyUpdatedMesh());
        }
    }

    public void setVoxel(Vector3Int pos, VOXELTYPE blocktype)
    {
        Vector3Int idexer = pos - new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        modeldata[idexer.x, idexer.y, idexer.z] = blocktype;
        Thread t = new Thread(voxelGenerationTask);
        t.Priority = System.Threading.ThreadPriority.AboveNormal;
        t.Start();
    }

    /************Purely Task/Threaded Code************/
    
    private Task currentTask;

    static Mutex[] mutexes = new Mutex[10];

    public static void mutexesInit()
    {
        for(int i = 0; i < mutexes.Length; i++)
        {
            mutexes[i] = new Mutex();
        }
    }

    void modelGenerationTask()
    {
        int mutexindex = WaitHandle.WaitAny(mutexes);
        modeldata = ChunkModelGenerator.generateSimpleFunction(bounds);
        mutexes[mutexindex].ReleaseMutex();
    }
    
    private bool meshNeedsApply = false;
    List<Vector3> outputvertices;
    List<Vector2> outputuvs;
    List<int> outputtriangles;
    
    void voxelGenerationTask()
    {
        int mutexindex = WaitHandle.WaitAny(mutexes);
        int vertexindex = 0;
        long temp1 = System.DateTime.Now.Ticks; // Debug
        outputvertices = new List<Vector3>();
        outputuvs = new List<Vector2>();
        outputtriangles = new List<int>();
        for (int x = 0; x < bounds.size.x; x++) // put stuff in the model
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int z = 0; z < bounds.size.z; z++)
                {
                    Vector3Int p = new Vector3Int(x, y, z);
                    if (modeldata[x, y, z] != VOXELTYPE.NONE)
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            if (isVoxelSideVisible(p, (DIRECTION)d))
                            {
                                VoxelBase vb = VoxelData.getVoxelType(modeldata[x,y,z]);
                                outputvertices.AddRange(vb.makeVoxelSideVertsAt(p, (DIRECTION)d).ToArray());
                                outputuvs.AddRange(vb.getVoxelUVs((DIRECTION)d).ToArray());
                                outputtriangles.AddRange(vb.getTriangles(ref vertexindex).ToArray());
                            }
                        }
                    }
                }
            }
        }
        meshNeedsApply = true;
        Debug.Log("That took " + (((System.DateTime.Now.Ticks - temp1) * 100f) / 1000000000) + " seconds"); // Debug
        mutexes[mutexindex].ReleaseMutex();
    }

    /***********************End of Threaded Tasks***************************/

    bool isPerimeterVoxelSide(Vector3Int pos, DIRECTION dir) // returns true if the voxel represented by the position is on the bounds of the chunk
    {
        return (pos.x == 0 && dir == DIRECTION.X_NEG) ||
               (pos.y == 0 && dir == DIRECTION.Y_NEG) || 
               (pos.z == 0 && dir == DIRECTION.Z_NEG) || 
               (pos.x == bounds.size.x - 1 && dir == DIRECTION.X_POS) || 
               (pos.y == bounds.size.y - 1 && dir == DIRECTION.Y_POS) ||
               (pos.z == bounds.size.z - 1 && dir == DIRECTION.Z_POS);
    }

    //////////////////////////////////////////////////////////////////////
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
    //////////////////////////////////////////////////////////////////////
    IEnumerator applyUpdatedMesh()
    {
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(outputvertices);
        m.SetUVs(0, outputuvs);
        m.SetTriangles(outputtriangles, 0);
        m.RecalculateNormals();
        meshFilter.mesh = m;
        meshCollider.sharedMesh = m;
        meshNeedsApply = false;
        yield return 0;
    }
}
