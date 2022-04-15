using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime;
using System.Linq;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;

namespace WorldGeneration
{
    public static class WorldGenerationData
    {
        public const float MaxRenderDistance = 100f;

        public const int MaxActiveChunkCount = 1000;

        public static readonly Vector3Int[] Directions = new Vector3Int[]
        {
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.up,
            Vector3Int.back,
            Vector3Int.forward
        };

        public static readonly Vector3Int ChunkSize = new Vector3Int()
        {
            x = 64,
            y = 64,
            z = 64
        };
    }
}

