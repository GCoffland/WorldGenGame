using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using WorldGeneration;

namespace WorldGeneration
{
    public static class ClassExtensions
    {
        /**************CHUNK INDEXING AND POSITIONING**************/
        /// <summary>
        /// Rounds the given vector to the postion of the chunk that contains the vector
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world postion of the chunk that contains the input vector</returns>
        public static Vector3Int RoundToChunkPos(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / WorldGenerationGlobals.ChunkSize.x) * WorldGenerationGlobals.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / WorldGenerationGlobals.ChunkSize.y) * WorldGenerationGlobals.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / WorldGenerationGlobals.ChunkSize.z) * WorldGenerationGlobals.ChunkSize.z);
        }

        /// <summary>
        /// Rounds the given vector to the postion of the chunk that contains the vector
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world postion of the chunk that contains the input vector</returns>
        public static Vector3Int RoundToChunkPos(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / WorldGenerationGlobals.ChunkSize.x) * WorldGenerationGlobals.ChunkSize.x,
                                  Mathf.FloorToInt(vec.y / WorldGenerationGlobals.ChunkSize.y) * WorldGenerationGlobals.ChunkSize.y,
                                  Mathf.FloorToInt(vec.z / WorldGenerationGlobals.ChunkSize.z) * WorldGenerationGlobals.ChunkSize.z);
        }

        /// <summary>
        /// Modulates the given vector with the global chunk size, to return the chunk-relative block position
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The block position within a chunk pointed to by vec</returns>
        public static Vector3Int WorldPosToBlockIndex(this Vector3Int vec)
        {
            return new Vector3Int(((vec.x % WorldGenerationGlobals.ChunkSize.x) + WorldGenerationGlobals.ChunkSize.x) % WorldGenerationGlobals.ChunkSize.x,
                ((vec.y % WorldGenerationGlobals.ChunkSize.y) + WorldGenerationGlobals.ChunkSize.y) % WorldGenerationGlobals.ChunkSize.y,
                ((vec.z % WorldGenerationGlobals.ChunkSize.z) + WorldGenerationGlobals.ChunkSize.z) % WorldGenerationGlobals.ChunkSize.z);
        }

        /// <summary>
        /// Scales the vector from chunk-index-space to world-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The world position of the chunk that is referred to by vec</returns>
        public static Vector3Int ChunkIndexToWorldPos(this Vector3Int vec)
        {
            return new Vector3Int(vec.x * WorldGenerationGlobals.ChunkSize.x,
                                  vec.y * WorldGenerationGlobals.ChunkSize.y,
                                  vec.z * WorldGenerationGlobals.ChunkSize.z);
        }

        /// <summary>
        /// Scales the vector from world-space to chunk-index-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The chunk-index of the chunk that is at the world-space location of vec</returns>
        public static Vector3Int WorldPosToChunkIndex(this Vector3Int vec)
        {
            return new Vector3Int(Mathf.FloorToInt((float)vec.x / WorldGenerationGlobals.ChunkSize.x),
                                  Mathf.FloorToInt((float)vec.y / WorldGenerationGlobals.ChunkSize.y),
                                  Mathf.FloorToInt((float)vec.z / WorldGenerationGlobals.ChunkSize.z));
        }

        /// <summary>
        /// Scales the vector from world-space to chunk-index-space
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>The chunk-index of the chunk that is at the world-space location of vec</returns>
        public static Vector3Int WorldPosToChunkIndex(this Vector3 vec)
        {
            return new Vector3Int(Mathf.FloorToInt(vec.x / WorldGenerationGlobals.ChunkSize.x),
                                  Mathf.FloorToInt(vec.y / WorldGenerationGlobals.ChunkSize.y),
                                  Mathf.FloorToInt(vec.z / WorldGenerationGlobals.ChunkSize.z));
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
            return arr[(1 + x) + ((1 + y) * (WorldGenerationGlobals.ChunkSize.x + 2)) + ((1 + z) * (((WorldGenerationGlobals.ChunkSize.x) + 2) * ((WorldGenerationGlobals.ChunkSize.y) + 2)))];
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
            arr[(1 + x) + ((1 + y) * (WorldGenerationGlobals.ChunkSize.x + 2)) + ((1 + z) * (((WorldGenerationGlobals.ChunkSize.x) + 2) * ((WorldGenerationGlobals.ChunkSize.y) + 2)))] = value;
        }

        public static void ReverseInGroups<T>(this T[] arr, int group_size)
        {
            if (arr.Length % group_size != 0)
            {
                throw new ArgumentException("Invalid group_size, array size must be a multiple of group_size");
            }
            int groups = arr.Length / group_size;
            T[] temp = new T[group_size];
            for (int i = 0; i < groups / 2; i++)
            {
                Array.Copy(arr, group_size * i, temp, 0, group_size);
                Array.Copy(arr, arr.Length - (group_size * (i + 1)), arr, group_size * i, group_size);
                Array.Copy(temp, 0, arr, arr.Length - (group_size * (i + 1)), group_size);
            }
        }
    }
}
