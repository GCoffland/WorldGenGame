using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateRecorder : MonoBehaviour
{
    public float TotalFPS;
    public float CurrentFPS;
    public float LastXFrames;
    public int X = 0;

    private float[] record;
    private int recordIndex = 0;

    private void OnValidate()
    {
        record = new float[X];
    }

    // Update is called once per frame
    void Update()
    {
        TotalFPS = Time.frameCount / Time.time;
        CurrentFPS = 1 / Time.deltaTime;
        if(X > 0)
        {
            record[recordIndex] = 1 / Time.deltaTime;
            LastXFrames += record[recordIndex] / record.Length;
            if (recordIndex == record.Length - 1)
            {
                recordIndex = 0;
            }
            else
            {
                recordIndex++;
            }
            LastXFrames -= record[recordIndex] / record.Length;
        }
    }
}
