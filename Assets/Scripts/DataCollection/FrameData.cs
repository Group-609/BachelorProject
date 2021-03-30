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
    public float rotationW;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float time;

    public FrameData(float x, float y, float z, float rotationW, float rotationX, float rotationY, float rotationZ, float time)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.rotationW = rotationW;
        this.rotationX = rotationX;
        this.rotationY = rotationY;
        this.rotationZ = rotationZ;
    }
}
