﻿using System.Collections;
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
            for (int x = 0; x < WorldGenerationGlobals.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerationGlobals.ChunkSize.y; y++)
                {
                    for (int z = 0; z < WorldGenerationGlobals.ChunkSize.z; z++)
                    {
                        uint block_type = 0;
                        float noise_val = cellular_noise.GetNoise(x + globalBounds.position.x, y + globalBounds.position.y, z + globalBounds.position.z);
                        if (noise_val < 0.25f)
                        {
                            if(noise_val > 0.0f)
                            {
                                block_type = (uint)WorldGenerationGlobals.blockIdByName["Dirt"];
                            }
                            else if (noise_val >= -1f)
                            {
                                block_type = (uint)WorldGenerationGlobals.blockIdByName["Stone"];
                            }
                            else
                            {
                                block_type = (uint)WorldGenerationGlobals.blockIdByName["Debug"];
                            }
                        }
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