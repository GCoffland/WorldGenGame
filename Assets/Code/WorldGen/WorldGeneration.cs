using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime;
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
    public enum VOXELTYPE
    {
        NONE,
        DIRT,
        GRASS
    };

    [GenerateHLSL(PackingRules.Exact, false)]
    unsafe public struct VertexBufferStruct
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texCoord;
    }

    [GenerateHLSL(PackingRules.Exact, false)]
    public static class Constants
    {
        public const float TNF = 1f/4f; // Texture Normalization Factor (1 / the number of textures in the x and y directions)

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
        /// <summary>
        /// Rounds the given vector to the postion of the chunk that contains the vector
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world postion of the chunk that contains the input vector</returns>
        public static Vector3Int RoundToChunkPos(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x) * Constants.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y) * Constants.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z) * Constants.ChunkSize.z);
        }

        /// <summary>
        /// Rounds the given vector to the postion of the chunk that contains the vector
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world postion of the chunk that contains the input vector</returns>
        public static Vector3Int RoundToChunkPos(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x) * Constants.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y) * Constants.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z) * Constants.ChunkSize.z);
        }

        /// <summary>
        /// Modulates the given vector with the global chunk size, to return the chunk-relative block position
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The block position within a chunk pointed to by vec</returns>
        public static Vector3Int WorldPosToBlockIndex(this Vector3Int vec)
        {
            return new Vector3Int(((vec.x % Constants.ChunkSize.x) + Constants.ChunkSize.x) % Constants.ChunkSize.x,
                ((vec.y % Constants.ChunkSize.y) + Constants.ChunkSize.y) % Constants.ChunkSize.y,
                ((vec.z % Constants.ChunkSize.z) + Constants.ChunkSize.z) % Constants.ChunkSize.z);
        }

        /// <summary>
        /// Scales the vector from chunk-index-space to world-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world position of the chunk that is referred to by vec</returns>
        public static Vector3Int ChunkIndexToWorldPos(this Vector3Int vec)
        {
            return new Vector3Int(vec.x * Constants.ChunkSize.x,
                                  vec.y * Constants.ChunkSize.y,
                                  vec.z * Constants.ChunkSize.z);
        }

        /// <summary>
        /// Scales the vector from world-space to chunk-index-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The chunk-index of the chunk that is at the world-space location of vec</returns>
        public static Vector3Int WorldPosToChunkIndex(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt((float)vec.x / Constants.ChunkSize.x),
                                  Mathf.FloorToInt((float)vec.y / Constants.ChunkSize.y),
                                  Mathf.FloorToInt((float)vec.z / Constants.ChunkSize.z));
        }

        /// <summary>
        /// Scales the vector from world-space to chunk-index-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The chunk-index of the chunk that is at the world-space location of vec</returns>
        public static Vector3Int WorldPosToChunkIndex(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / Constants.ChunkSize.x),
                                  Mathf.FloorToInt(vec.y / Constants.ChunkSize.y),
                                  Mathf.FloorToInt(vec.z / Constants.ChunkSize.z));
        }

        /// <summary>
        /// Converts a Vector3 to a Vector3Int, truncating the floating point components
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The converted Vector3Int</returns>
        public static Vector3Int ToVector3Int(this Vector3 vec)
        {
            return new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
        }

        /// <summary>
        /// Get the value in an NativeArray as if it were a 3D array with 1-width padding on each side that is ignored
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>The value at x, y, z</returns>
        public static T GetAsChunk<T>(this NativeArray<T> arr, int x, int y, int z) where T : struct
        {
            return arr[(1 + x) + ((1 + y) * (Constants.ChunkSize.x + 2)) + ((1 + z) * (((Constants.ChunkSize.x) + 2) * ((Constants.ChunkSize.y) + 2)))];
        }

        /// <summary>
        /// Set the value in an NativeArray as if it were a 3D array with 1-width padding on each side that is ignored
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        public static void SetAsChunk<T>(this NativeArray<T> arr, int x, int y, int z, T value) where T : struct
        {
            arr[(1 + x) + ((1 + y) * (Constants.ChunkSize.x + 2)) + ((1 + z) * (((Constants.ChunkSize.x) + 2) * ((Constants.ChunkSize.y) + 2)))] = value;
        }
    }
}

