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
    public struct ModelGenJob : IJob
    {
        public ModelGenJob(ref NativeArray<uint> block_map, BoundsInt global_bounds)
        {
            blockMap = block_map;
            globalBounds = global_bounds;
        }

        public BoundsInt globalBounds
        {
            get;
            private set;
        }

        public NativeArray<uint> blockMap;

        public void Execute()
        {
            FastNoiseLite cellular_noise = new FastNoiseLite();
            cellular_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            for (int x = -1; x < WorldGenerationGlobals.ChunkSize.x + 1; x++)
            {
                for (int y = -1; y < WorldGenerationGlobals.ChunkSize.y + 1; y++)
                {
                    for (int z = -1; z < WorldGenerationGlobals.ChunkSize.z + 1; z++)
                    {
                        uint block_type = default;
                        float noise_val = cellular_noise.GetNoise(x + globalBounds.position.x - 1, y + globalBounds.position.y - 1, z + globalBounds.position.z - 1);
                        if (noise_val < 0.25f)
                        {
                            block_type = 1;
                        }
                        else
                        {
                            block_type = 0;
                        }
                        blockMap[(x + 1) + ((y + 1) * (WorldGenerationGlobals.ChunkSize.x + 2)) + ((z + 1) * ((WorldGenerationGlobals.ChunkSize.x + 2) * (WorldGenerationGlobals.ChunkSize.y + 2)))] = block_type;
                    }
                }
            }
        }
    }

    public class ModelGenerator : MonoBehaviour
    {
        internal static ModelGenerator Singleton;

        internal static int seed;

        public async Task<NativeArray<uint>> GenerateBlockmap(NativeArray<uint> block_map, BoundsInt global_bounds)
        {
            ModelGenJob gen = new ModelGenJob(ref block_map, global_bounds);
            JobHandle handle = gen.Schedule();
            while(!handle.IsCompleted) { await Task.Yield(); }
            handle.Complete();
            return block_map;
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}