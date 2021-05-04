using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ValidationData
{
    public string CDR;
    public string CDD;

    public string time;

    public ValidationData(float CDR, float CDD, float time)
    {
        this.CDD = CDD.ToString("00000");
        this.CDR = CDR.ToString("00000");
        this.time = time.ToString("0000.0");
    }
}
