using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    List<List<long>> startTimes = new List<List<long>>();
    List<List<long>> endTimes = new List<List<long>>();
    
    public int newSet()
    {
        startTimes.Add(new List<long>());
        endTimes.Add(new List<long>());
        return endTimes.Count - 1;
    }

    public void startTimeStamp(int timerindex)
    {
        startTimes[timerindex].Add(System.DateTime.Now.Ticks);
    }

    public void endTimeStamp(int timerindex)
    {
        endTimes[timerindex].Add(System.DateTime.Now.Ticks);
    }

    public float getAverageTime(int timerindex)
    {
        if (startTimes[timerindex].Count != endTimes[timerindex].Count)
        {
            Debug.Log("finish all your timers first");
            return 0;
        }
        long temp = 0;
        for(int i = 0; i < startTimes[timerindex].Count; i++)
        {
            temp += endTimes[timerindex][i] - startTimes[timerindex][i];
        }
        return (((((float)temp) / ((float)endTimes[timerindex].Count)) * 100f) / 1000000000);
    }
}
