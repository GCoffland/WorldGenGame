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
    [GenerateHLSL(PackingRules.Exact, false)]
    public enum DIRECTION
    {
        X_NEG,
        X_POS,
        Y_NEG,
        Y_POS,
        Z_NEG,
        Z_POS,
    };

    [GenerateHLSL(PackingRules.Exact, false)]
    public struct VertexBufferStruct
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texCoord;
    }

    [GenerateHLSL(PackingRules.Exact, false)]
    public static class WorldGenerationGlobals
    {

        public const string blockTexturePath = "VoxelTextures/";

        public static readonly Texture2D atlas;

        static WorldGenerationGlobals()
        {
            Cubemap[] maps = Resources.LoadAll<Cubemap>(blockTexturePath);

            int width = 0;
            int height = 0;
            foreach (Cubemap c in maps)
            {
                width += c.width;
                height = Math.Max(height, c.height * 6);
            }

            atlas = new Texture2D(width, height, DefaultFormat.LDR, TextureCreationFlags.None);
            atlas.PackCubemaps(maps);

            Debug.Log("Initialized constants");
        }

        public const float MaxRenderDistance = 100f;

        public const int MaxActiveChunkCount = 1000;

        public static readonly VertexAttributeDescriptor[] VertexBufferLayout = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

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

        public static int BlockMapLength
        {
            get
            {
                return (ChunkSize.x + 2) * (ChunkSize.y + 2) * (ChunkSize.z + 2);
            }
        }


        public static int MaxPossibleVerticies
        {
            get
            {
                return (ChunkSize.x * ChunkSize.y * ChunkSize.z) / 2;
            }
        }
    }
}

