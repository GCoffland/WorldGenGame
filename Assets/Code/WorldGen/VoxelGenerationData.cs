using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

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
    public static class VoxelGenerationData
    {
        public const string blockTexturePath = "VoxelTextures/";

        public static readonly Texture2D atlas;

        static VoxelGenerationData()
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

        public static readonly VertexAttributeDescriptor[] VertexBufferLayout = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        public static int MaxPossibleVerticies
        {
            get
            {
                return (WorldGenerationData.ChunkSize.x * WorldGenerationData.ChunkSize.y * WorldGenerationData.ChunkSize.z) / 2;
            }
        }
    }

}
