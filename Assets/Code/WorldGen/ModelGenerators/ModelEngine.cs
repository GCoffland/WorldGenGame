using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;
using System.Reflection;

namespace WorldGeneration
{
    public static class ModelEngine
    {
        public static Dictionary<uint, Func<int, int, int, uint>> Engines = new Dictionary<uint, Func<int, int, int, uint>>();

        private static FastNoiseLite noise;

        static ModelEngine()
        {
            MethodInfo[] methods = MethodBase.GetCurrentMethod().DeclaringType.GetMethods();
            uint i = 0;
            foreach (MethodInfo method in methods)
            {
                if (method.Name.Contains("GetBlock"))
                {
                    Debug.Log("Adding " + method.Name + " as: " + i);
                    Func<int, int, int, uint> func = (int x, int y, int z) =>
                    {
                        return (uint)method.Invoke(null, new object[] { x, y, z });
                    };
                    Engines.Add(i, func);
                    i++;
                }
            }
            noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        }

        public static uint GetBlock(int x, int y, int z)
        {
            float noise_val = noise.GetNoise(x, y, z);
            if (noise_val < 0.25f)
            {
                if (noise_val > 0.0f)
                {
                    return (uint)WorldGenerationGlobals.blockIdByName["Dirt"];
                }
                else if (noise_val >= -1f)
                {
                    return (uint)WorldGenerationGlobals.blockIdByName["Stone"];
                }
                else
                {
                    return (uint)WorldGenerationGlobals.blockIdByName["Debug"];
                }
            }
            return 0;
        }
    }
}