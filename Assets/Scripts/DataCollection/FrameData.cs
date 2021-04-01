using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FrameData
{
    public float x;
    public float y;
    public float z;
    public float time;

    public FrameData(float x, float y, float z, float time)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.time = time;
    }
}
