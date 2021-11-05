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
        public ModelGenJob(ref NativeArray<VOXELTYPE> block_map, Vector3Int size, Vector3Int global_pos)
        {
            blockMap = block_map;
            this.size = size;
            globalPosition = global_pos;
            ready = false;
        }

        public Vector3Int globalPosition
        {
            get;
            private set;
        }
        public Vector3Int size
        {
            get;
            private set;
        }
        public NativeArray<VOXELTYPE> blockMap;
        public bool ready
        {
            get;
            private set;
        }

        public void Execute()
        {
            FastNoiseLite cellular_noise = new FastNoiseLite();
            cellular_noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            FastNoiseLite opensimplex_noise = new FastNoiseLite();
            cellular_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            Func<float,float,float,float> gradient = (x,y,z) => {
                return Math.Clamp(y/10, -1f, 1f);
            };
            for (int x = -1; x < size.x + 1; x++)
            {
                for (int y = -1; y < size.y + 1; y++)
                {
                    for (int z = -1; z < size.z + 1; z++)
                    {
                        VOXELTYPE block_type = default;
                        float noise_val = (cellular_noise.GetNoise(x + globalPosition.x - 1, y + globalPosition.y - 1, z + globalPosition.z - 1)
                            + opensimplex_noise.GetNoise(x + globalPosition.x - 1, y + globalPosition.y - 1, z + globalPosition.z - 1)
                            + gradient(x + globalPosition.x - 1, y + globalPosition.y - 1, z + globalPosition.z - 1));
                        noise_val += 3;
                        noise_val = noise_val / 3;
                        noise_val -= 1;
                        if (noise_val < 0f)
                        {
                            block_type = VOXELTYPE.DIRT;
                        }
                        else
                        {
                            block_type = VOXELTYPE.NONE;
                        }
                        blockMap[(x + 1) + ((y + 1) * (size.x + 2)) + ((z + 1) * ((size.x + 2) * (size.y + 2)))] = block_type;
                    }
                }
            }
            ready = true;
        }
    }

    public class ChunkModelGenerator : MonoBehaviour
    {
        internal static ChunkModelGenerator Singleton;

        internal static int seed;

        public static JobHandle GenerateBlockmap(ref NativeArray<VOXELTYPE> block_map, Vector3Int size, Vector3Int global_position)
        {
            ModelGenJob gen = new ModelGenJob(ref block_map, size, global_position);
            return gen.Schedule();
        }

        private void Awake()
        {
            Singleton = this;
        }
    }
}