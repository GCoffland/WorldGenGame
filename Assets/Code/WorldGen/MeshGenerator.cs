using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using WorldGeneration;
using System.Threading.Tasks;
using System.Linq;


namespace WorldGeneration
{
    public partial class MeshGenerator : MonoBehaviour
    {
        [SerializeField]
        internal ComputeShader computeShader;
        internal static MeshGenerator Singleton;

        private int[] dispatchArgs;

        private NativeArray<VertexBufferStruct> Verticies;
        private NativeArray<int> Quads;
        private NativeArray<int> BufferCounts;      //[0] is Verticies count, [1] is Quads count

        ComputeBuffer blockUVsbuffer;

        ComputeBuffer vertexbuffer;
        ComputeBuffer quadbuffer;
        ComputeBuffer blockmapbuffer;
        ComputeBuffer bufferlengthsbuffer;

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            Init();
            state = GeneratorState.Ready;
        }

        private void OnDestroy()
        {
            AsyncGPUReadback.WaitAllRequests();
            ReleaseResources();
        }

        public enum GeneratorState
        {
            Uninitialized,
            Ready,
            Busy
        };
        public GeneratorState state
        {
            get;
            private set;
        } = GeneratorState.Uninitialized;

        private int taskID = 0;

        Queue<int> requests = new Queue<int>();

        public static async Task GenerateMeshData(NativeArray<uint> blockmap, Mesh mesh)
        {
            int id = Interlocked.Increment(ref Singleton.taskID);
            Singleton.requests.Enqueue(id);
            //Debug.Log("Queued Task: " + id);
            while (Singleton.requests.Peek() != id)
            {
                await Task.Yield();
            }
            Singleton.state = GeneratorState.Busy;

            Singleton.SetDataAndDispatch(blockmap);
            List<AsyncGPUReadbackRequest> reqs = Singleton.QueueRequestData();
            await Singleton.WaitForRequests(reqs);
            Singleton.applyUpdatedMesh(mesh);

            Singleton.state = GeneratorState.Ready;
            Singleton.requests.Dequeue();
        }

        private void Init()
        {
            dispatchArgs = new int[3];
            uint[] temp = new uint[3];
            computeShader.GetKernelThreadGroupSizes(0, out temp[0], out temp[1], out temp[2]);
            dispatchArgs[0] = (WorldGenerationGlobals.ChunkSize.x / (int)temp[0]);
            dispatchArgs[1] = (WorldGenerationGlobals.ChunkSize.y / (int)temp[1]);
            dispatchArgs[2] = (WorldGenerationGlobals.ChunkSize.z / (int)temp[2]);

            var blockRects = WorldGenerationGlobals.blockData.ToArray();
            int maxBlockIndex = blockRects.Max((x) => { return x.Key; });
            NativeArray<Vector2> blockUVs = new NativeArray<Vector2>((maxBlockIndex + 1) * 6, Allocator.Persistent);
            for(int i = 0; i < blockRects.Length; i++)
            {
                if (blockRects[i].Value.blockData.Cubemap != null)
                {
                    for(int j = 0; j < 6; j++)
                    {
                        blockUVs[blockRects[i].Key * 6 + j] = blockRects[i].Value.atlasRect[5 - j].min;
                    }
                }
            }
            
            Verticies = new NativeArray<VertexBufferStruct>(WorldGenerationGlobals.MaxPossibleVerticies, Allocator.Persistent);
            Quads = new NativeArray<int>(WorldGenerationGlobals.MaxPossibleVerticies, Allocator.Persistent);
            BufferCounts = new NativeArray<int>(2, Allocator.Persistent);

            vertexbuffer = new ComputeBuffer(WorldGenerationGlobals.MaxPossibleVerticies / 4, 128, ComputeBufferType.Counter | ComputeBufferType.Structured);
            quadbuffer = new ComputeBuffer(WorldGenerationGlobals.MaxPossibleVerticies / 4, 16, ComputeBufferType.Counter | ComputeBufferType.Structured);
            bufferlengthsbuffer = new ComputeBuffer(2, 4);
            blockmapbuffer = new ComputeBuffer(WorldGenerationGlobals.BlockMapLength, 4, ComputeBufferType.Default);
            blockUVsbuffer = new ComputeBuffer(blockUVs.Length, 8, ComputeBufferType.Default);

            computeShader.SetBuffer(0, "VertexResult", vertexbuffer);
            computeShader.SetBuffer(0, "QuadResult", quadbuffer);
            computeShader.SetBuffer(0, "BufferLengths", bufferlengthsbuffer);
            computeShader.SetBuffer(0, "BlockMap", blockmapbuffer);
            computeShader.SetBuffer(0, "BlockUVs", blockUVsbuffer);
            computeShader.SetVector("TNF", WorldGenerationGlobals.TNF);
            blockUVsbuffer.SetData(blockUVs);

            if (blockUVs.IsCreated) blockUVs.Dispose();
        }

        private void ResetCounters()
        {
            vertexbuffer.SetCounterValue(0);
            quadbuffer.SetCounterValue(0);
            bufferlengthsbuffer.SetData<int>(new List<int>(new int[] { 0, 0 }));
        }

        private void SetDataAndDispatch(in NativeArray<uint> blockmap)
        {
            ResetCounters();

            blockmapbuffer.SetData(blockmap);
            computeShader.SetInts("DispatchArgs", dispatchArgs);

            computeShader.Dispatch(0, dispatchArgs[0], dispatchArgs[1], dispatchArgs[2]);
        }

        private List<AsyncGPUReadbackRequest> QueueRequestData()
        {
            List<AsyncGPUReadbackRequest> rets = new List<AsyncGPUReadbackRequest>();

            rets.Add(AsyncGPUReadback.RequestIntoNativeArray<VertexBufferStruct>(ref Verticies, vertexbuffer));
            rets.Add(AsyncGPUReadback.RequestIntoNativeArray<int>(ref Quads, quadbuffer));
            rets.Add(AsyncGPUReadback.RequestIntoNativeArray<int>(ref BufferCounts, bufferlengthsbuffer));

            return rets;
        }

        private async Task WaitForRequests(IList<AsyncGPUReadbackRequest> reqs)
        {
            bool reqs_pending = true;
            while (reqs_pending)
            {
                reqs_pending = false;
                foreach (AsyncGPUReadbackRequest req in reqs)
                {
                    if (!req.done)
                    {
                        reqs_pending = true;
                    }
                }
                if (reqs_pending)
                {
                    await Task.Yield();
                }
            }
            //Debug.Log("Verticies Used: " + BufferCounts[0] + "/" + BufferElementCapacity);
            //Debug.Log("Quads Used: " + BufferCounts[1] + "/" + BufferElementCapacity);
        }

        private void applyUpdatedMesh(Mesh mesh)
        {
            mesh.Clear();
            mesh.SetVertexBufferParams(BufferCounts[0], WorldGenerationGlobals.VertexBufferLayout);
            mesh.SetIndexBufferParams(BufferCounts[1], IndexFormat.UInt32);
            mesh.SetVertexBufferData(Verticies, 0, 0, BufferCounts[0], flags: (MeshUpdateFlags)15);
            mesh.SetIndexBufferData(Quads, 0, 0, BufferCounts[1], flags: (MeshUpdateFlags)15);
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, BufferCounts[1], MeshTopology.Quads));
            mesh.RecalculateBounds();
        }

        private void ReleaseResources()
        {
            if (Verticies.IsCreated) Verticies.Dispose();
            if (Quads.IsCreated) Quads.Dispose();
            if (BufferCounts.IsCreated) BufferCounts.Dispose();
            vertexbuffer?.Release();
            quadbuffer?.Release();
            blockmapbuffer?.Release();
            bufferlengthsbuffer?.Release();
            blockUVsbuffer?.Release();
        }
    }
}
