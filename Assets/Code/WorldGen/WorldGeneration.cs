using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldGeneration
{
    public enum DIRECTION
    {
        X_NEG,
        X_POS,
        Y_NEG,
        Y_POS,
        Z_NEG,
        Z_POS,
    };

    public enum VOXELTYPE
    {
        NONE,
        DIRT,
        GRASS
    };

    public struct VertexBufferStruct
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 TexCoord;
    }

    public static class Constants
    {
        public const float TNF = 0.25f; // Texture Normalization Factor (1 / the number of textures in the x and y directions)

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

        public static readonly Vector3Int ChunkSize = new Vector3Int()
        {
            x = 64,
            y = 64,
            z = 64
        };
    }

    public static class ClassAdditions
    {
        /**************CHUNK INDEXING AND POSITIONING**************/
        public static Vector3Int RoundToChunkChunkPos(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x) * Constants.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y) * Constants.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z) * Constants.ChunkSize.z);
        }

        public static Vector3Int RoundToChunkChunkPos(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x) * Constants.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y) * Constants.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z) * Constants.ChunkSize.z);
        }

        public static Vector3Int ChunkIndexToWorldPos(this Vector3Int vec)
        {
            return new Vector3Int(vec.x * Constants.ChunkSize.x,
                                  vec.y * Constants.ChunkSize.y,
                                  vec.z * Constants.ChunkSize.z);
        }

        public static Vector3Int WorldPosToChunkIndex(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt((float)vec.x / Constants.ChunkSize.x),
                                  Mathf.FloorToInt((float)vec.y / Constants.ChunkSize.y),
                                  Mathf.FloorToInt((float)vec.z / Constants.ChunkSize.z));
        }

        public static Vector3Int WorldPosToChunkIndex(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x),
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y),
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z));
        }

        public static Vector3Int ToVector3Int(this Vector3 vec)
        {
            return new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
        }
    }
}

