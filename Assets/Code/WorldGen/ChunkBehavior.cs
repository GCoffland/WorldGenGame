using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading;
using System.Threading.Tasks;

public class ChunkBehavior : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BoundsInt bounds;
    public MeshCollider meshCollider;
    public ComputeShader computeShader;

    private NativeArray<VertexBufferStruct> Verticies;
    private uint VertexCount;
    private NativeArray<uint> Quads;
    private uint QuadCount;

    private bool meshNeedsApply = false;
    ComputeBuffer vertexbuffer;
    ComputeBuffer quadbuffer;
    ComputeBuffer blockmapbuffer;
    ComputeBuffer vertcountbuffer;
    ComputeBuffer quadcountbuffer;
    readonly int[] DispatchArgs = new int[] { 1, 1, 1 };
    AsyncGPUReadbackRequest vertexRequest;
    AsyncGPUReadbackRequest quadRequest;

    readonly uint[] blockmap = new uint[]
    {
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 1, 0, 1, 0, 0,
        0, 0, 1, 0, 0, 0,
        0, 1, 0, 1, 1, 0,
        0, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
    };

    private void Awake()
    {
        Verticies = new NativeArray<VertexBufferStruct>(128 * 32 * 16, Allocator.Persistent);
        Quads = new NativeArray<uint>(128 * 32 * 16, Allocator.Persistent);
    }

    // Start is called before the first frame update
    void Start()
    {
        vertexbuffer = new ComputeBuffer(128 * 32, 128, ComputeBufferType.Counter | ComputeBufferType.Structured);
        quadbuffer = new ComputeBuffer(128 * 32, 16, ComputeBufferType.Counter | ComputeBufferType.Structured);
        blockmapbuffer = new ComputeBuffer(blockmap.Length, 4, ComputeBufferType.Default);
        vertcountbuffer = new ComputeBuffer(1, 4);
        quadcountbuffer = new ComputeBuffer(1, 4);

        blockmapbuffer.SetData(blockmap);
        computeShader.SetBuffer(0, "VertexResult", vertexbuffer);
        computeShader.SetBuffer(0, "QuadResult", quadbuffer);
        computeShader.SetBuffer(0, "VertexCount", vertcountbuffer);
        computeShader.SetBuffer(0, "QuadCount", quadcountbuffer);
        computeShader.SetBuffer(0, "BlockMap", blockmapbuffer);
        computeShader.SetInts("DispatchArgs", DispatchArgs);
        computeShader.Dispatch(0, DispatchArgs[0], DispatchArgs[1], DispatchArgs[2]);
        vertexRequest = AsyncGPUReadback.RequestIntoNativeArray<VertexBufferStruct>(ref Verticies, vertexbuffer);
        quadRequest = AsyncGPUReadback.RequestIntoNativeArray<uint>(ref Quads, quadbuffer);
        StartCoroutine(WaitForRequests());
    }

    public void OnDestroy()
    {
        vertexbuffer.Release();
        quadbuffer.Release();
        Verticies.Dispose();
        Quads.Dispose();
        blockmapbuffer.Release();
        vertcountbuffer.Release();
        quadcountbuffer.Release();
    }

    IEnumerator WaitForRequests()
    {
        while(!vertexRequest.done || !quadRequest.done)
        {
            yield return 0;
        }
        uint[] temp = new uint[1];
        vertcountbuffer.GetData(temp);
        VertexCount = temp[0];
        quadcountbuffer.GetData(temp);
        QuadCount = temp[0];
        applyUpdatedMesh();
        //WorldBehavior.instance.chunkCurrentlyGenerating = false;
    }

    void applyUpdatedMesh()
    {
        Mesh m = meshFilter.mesh;
        m.Clear();
        m.SetVertexBufferParams(Verticies.Length, VoxelData.VertexBufferLayout);
        m.SetIndexBufferParams(Quads.Length, IndexFormat.UInt32);
        m.SetVertexBufferData(Verticies, 0, 0, (int)VertexCount, flags: (MeshUpdateFlags)15);
        m.SetIndexBufferData(Quads, 0, 0, (int)QuadCount, flags: (MeshUpdateFlags)15);
        m.SetSubMesh(0, new SubMeshDescriptor(0, (int)QuadCount, MeshTopology.Quads));
        m.RecalculateBounds();
        meshFilter.mesh = m;
        meshCollider.sharedMesh = m;
        meshNeedsApply = false;
    }

    public void removeBlockAt(Vector3Int localpos)
    {
        if(ChunkModelGenerator.voxelAtPoint(bounds.min + localpos) == VOXELTYPE.NONE)
        {
            UnityEngine.Debug.Log("Tried to remove a block that does not exist at " + localpos);
        }
        else
        {
            //removeVoxelAt(localpos);
            WorldBehavior.instance.blockChangeLog[bounds.min + localpos] = VOXELTYPE.NONE;
        }
    }

    public void addVoxelAt(Vector3Int pos, VOXELTYPE blocktype)
    {

    }
 
    /************Purely Task/Threaded Code************/
    static Mutex chunkMutex = new Mutex();

    const int numOfThreads = 8;

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
        if (ChunkModelGenerator.voxelAtPoint(bounds.min + pos) == VOXELTYPE.NONE) // if voxel is air
        {
            return false;
        }
        else if (isPerimeterVoxelSide(pos, dir)) // if voxel is a on the chunk bounds
        {
            return ChunkModelGenerator.voxelAtPoint(bounds.min + pos + VoxelData.DIRECTIONVECTORS[dir]) == VOXELTYPE.NONE;
        }
        else if (ChunkModelGenerator.voxelAtPoint(bounds.min + pos + VoxelData.DIRECTIONVECTORS[dir]) == VOXELTYPE.NONE)
        {
            return true;
        }
        return false;
    }
}
