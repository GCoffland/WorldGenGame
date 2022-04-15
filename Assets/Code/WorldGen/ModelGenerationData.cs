using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGeneration
{
    public static class ModelGenerationData
    {
        public static int BlockMapLength
        {
            get
            {
                return (WorldGenerationData.ChunkSize.x + 2) * (WorldGenerationData.ChunkSize.y + 2) * (WorldGenerationData.ChunkSize.z + 2);
            }
        }
    }
}
