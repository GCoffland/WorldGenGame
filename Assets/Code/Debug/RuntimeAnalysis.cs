using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuntimeAnalysis
{
    public static List<float> modelCreationTimes = new List<float>();
    public static List<float> voxelCreationTimes = new List<float>();
    public static List<float> meshCreationTimes = new List<float>();
    public static List<float> meshAssigningTimes = new List<float>();

    public static float getAverageModelCreationTimes()
    {
        float temp = 0;
        foreach(float f in modelCreationTimes)
        {
            temp += f;
        }
        return temp / modelCreationTimes.Count;
    }

    public static float getAverageVoxelCreationTimes()
    {
        float temp = 0;
        foreach (float f in voxelCreationTimes)
        {
            temp += f;
        }
        return temp / voxelCreationTimes.Count;
    }

    public static float getAverageMeshCreationTimes()
    {
        float temp = 0;
        foreach (float f in meshCreationTimes)
        {
            temp += f;
        }
        return temp / meshCreationTimes.Count;
    }

    public static float getAverageMeshAssigningTimes()
    {
        float temp = 0;
        foreach (float f in meshAssigningTimes)
        {
            temp += f;
        }
        return temp / meshAssigningTimes.Count;
    }
}
