using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public class Sync : Node
{
    private readonly Stopwatch ping = new Stopwatch();
    private readonly List<float> cumulativeList = new List<float>();
    public float Delay { get; private set; } = 0;

    public void TestDelay()
    {
        ping.Restart();
        Rpc("Ping");
    }

    [Remote]
    void Ping()
    {
        Rpc("ReturnPing");
    }

    [Remote]
    void ReturnPing()
    {
        ping.Stop();
        cumulativeList.Add(ping.ElapsedMilliseconds);
    }

    public float GetDelay()
    {
        if (cumulativeList.Count == 0) return Delay;

        float delay = 0;
        int i;

        for(i = 0; i < cumulativeList.Count; i++)
        {
            delay += cumulativeList[i];
        }

        var d = (delay / cumulativeList.Count);
        return Delay = d / 2;
    }

    public void ClearList()
    {
        cumulativeList.Clear();
    }
}