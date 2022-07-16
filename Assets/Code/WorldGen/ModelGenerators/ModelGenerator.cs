using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorldGeneration;
using Unity.Jobs;
using Unity.Collections;

namespace WorldGeneration
{
    public class ModelGenerator : MonoBehaviour
    {
        internal static ModelGenerator Singleton;

        internal static int seed;

        public async Task<NativeArray<uint>> GenerateBlockmap(NativeArray<uint> block_map, BoundsInt global_bounds)
        {
            JobHandle handle;
            var gen = new ModelGenerationJob(ref block_map, global_bounds, 0);
            handle = gen.Schedule();
            while(!handle.IsCompleted) { await Task.Yield(); }
            handle.Complete();
            return block_map;
        }

        private void Awake()
        {
            Singleton = this;
        }
    }

    internal struct ModelGenerationJob : IJob
    {
        internal ModelGenerationJob(ref NativeArray<uint> block_map, BoundsInt global_bounds, uint model_engine_id)
        {
            blockMap = block_map;
            globalBounds = global_bounds;
            modelEngineId = model_engine_id;
        }

        private BoundsInt globalBounds;
        private NativeArray<uint> blockMap;
        private uint modelEngineId;

        public void Execute()
        {
            var engine = ModelEngine.Engines[modelEngineId];
            for (int x = 0; x < WorldGenerationGlobals.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerationGlobals.ChunkSize.y; y++)
                {
                    for (int z = 0; z < WorldGenerationGlobals.ChunkSize.z; z++)
                    {
                        uint block_type = engine(x + globalBounds.x, y + globalBounds.y, z + globalBounds.z);

                        blockMap.SetAsChunk(x, y, z, block_type);
                    }
                }
            }

            for (int x = 0; x < WorldGenerationGlobals.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerationGlobals.ChunkSize.y; y++)
                {
                    for (int z = 0; z < WorldGenerationGlobals.ChunkSize.z; z++)
                    {
                        if (y < WorldGenerationGlobals.ChunkSize.z - 1 &&
                            blockMap.GetAsChunk(x, y, z) == (uint)WorldGenerationGlobals.blockIdByName["Dirt"] &&
                            blockMap.GetAsChunk(x, y + 1, z) == 0)
                        {
                            blockMap.SetAsChunk(x, y, z, (uint)WorldGenerationGlobals.blockIdByName["Grass"]);
                        }
                    }
                }
            }
        }
    }
}