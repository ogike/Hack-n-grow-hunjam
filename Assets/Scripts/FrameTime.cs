using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimeValue
{
    public int frames;

    public float Seconds => frames * FrameTime.FrameTimeSeconds;
}

public class FrameTime
{
    public const int FramesPerSecond = 20;

    public const float FrameTimeSeconds = 1.0f / FramesPerSecond; //50 ms
}
