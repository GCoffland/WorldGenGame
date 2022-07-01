using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using WorldGeneration;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

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
        ComputeBuffer borderblockmapbuffer;
        ComputeBuffer bufferlengthsbuffer;

        private SemaphoreSlim queueSem = new SemaphoreSlim(1);

        public async Task<Mesh> GenerateMeshData(NativeArray<uint> blockmap, NativeArray<uint>[] borderblockmaps)
        {
            await queueSem.WaitAsync();

            Singleton.SetDataAndDispatch(blockmap, borderblockmaps);
            await Singleton.AsyncWaitForData();
            Mesh mesh = Singleton.CreateMeshFromCurrentData();

            queueSem.Release(1);
            return mesh;
        }

        private void Awake()
        {
            Singleton = this;
            Init();
        }

        private void OnDestroy()
        {
            AsyncGPUReadback.WaitAllRequests();
            ReleaseResources();
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
            borderblockmapbuffer = new ComputeBuffer(WorldGenerationGlobals.ChunkSideAreas.Sum(), sizeof(uint), ComputeBufferType.Default);
            blockUVsbuffer = new ComputeBuffer(blockUVs.Length, 8, ComputeBufferType.Default);

            computeShader.SetBuffer(0, "VertexResult", vertexbuffer);
            computeShader.SetBuffer(0, "QuadResult", quadbuffer);
            computeShader.SetBuffer(0, "BufferLengths", bufferlengthsbuffer);
            computeShader.SetBuffer(0, "BlockMap", blockmapbuffer);
            computeShader.SetBuffer(0, "BorderBlockMap", borderblockmapbuffer);
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

        private void SetDataAndDispatch(in NativeArray<uint> blockmap, in NativeArray<uint>[] borderblockmaps)
        {
            ResetCounters();

            blockmapbuffer.SetData(blockmap);
            int buffer_index = 0;
            for(int i = 0; i < borderblockmaps.Length; i++)
            {
                borderblockmapbuffer.SetData(borderblockmaps[i], 0, buffer_index, borderblockmaps[i].Length);
                buffer_index += borderblockmaps[i].Length;
            }
            computeShader.SetInts("DispatchArgs", dispatchArgs);

            computeShader.Dispatch(0, dispatchArgs[0], dispatchArgs[1], dispatchArgs[2]);
        }

        SemaphoreSlim dataRetrievalSem = new SemaphoreSlim(0, 3);

        private async Task AsyncWaitForData()
        {
            AsyncGPUReadback.RequestIntoNativeArray<VertexBufferStruct>(ref Verticies, vertexbuffer, (x) =>
                {
                    dataRetrievalSem.Release(1);
                });
            AsyncGPUReadback.RequestIntoNativeArray<int>(ref Quads, quadbuffer, (x) =>
                {
                    dataRetrievalSem.Release(1);
                });
            AsyncGPUReadback.RequestIntoNativeArray<int>(ref BufferCounts, bufferlengthsbuffer, (x) =>
                {
                    dataRetrievalSem.Release(1);
                });
            for(int i = 0; i < 3; i++)
            {
                await dataRetrievalSem.WaitAsync();
            }
        }

        private Mesh CreateMeshFromCurrentData()
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.SetVertexBufferParams(BufferCounts[0], WorldGenerationGlobals.VertexBufferLayout);
            mesh.SetIndexBufferParams(BufferCounts[1], IndexFormat.UInt32);
            mesh.SetVertexBufferData(Verticies, 0, 0, BufferCounts[0], flags: (MeshUpdateFlags)15);
            mesh.SetIndexBufferData(Quads, 0, 0, BufferCounts[1], flags: (MeshUpdateFlags)15);
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, BufferCounts[1], MeshTopology.Quads));
            mesh.RecalculateBounds();
            //Physics.BakeMesh(mesh.GetInstanceID(), true);      // can be called manually and multithreaded
            return mesh;
        }

        private void ReleaseResources()
        {
            if (Verticies.IsCreated) Verticies.Dispose();
            if (Quads.IsCreated) Quads.Dispose();
            if (BufferCounts.IsCreated) BufferCounts.Dispose();
            vertexbuffer?.Release();
            quadbuffer?.Release();
            blockmapbuffer?.Release();
            borderblockmapbuffer?.Release();
            bufferlengthsbuffer?.Release();
            blockUVsbuffer?.Release();
        }
    }
}
