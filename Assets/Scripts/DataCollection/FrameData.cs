using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FrameData
{
    public string x;
    public string y;
    public string z;
    public string time;

    public FrameData(float x, float y, float z, float time)
    {
        this.x = x.ToString("0.00"); ;
        this.y = y.ToString("0.00"); ;
        this.z = z.ToString("0.00"); ;
        this.time = time.ToString("0.00"); ;
    }
}
