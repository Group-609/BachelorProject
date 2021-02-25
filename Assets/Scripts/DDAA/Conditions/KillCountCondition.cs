using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCountCondition : ICondition
{
    public float ConditionValue
    {
        get => currentKillCount;
        set
        {
            currentKillCount = (int) value;
        }
    }

    private int currentKillCount;

    public float GetAdditiveValue(int[] conditionalValues, float[] additiveValues)
    {
        for(int i=0; i<conditionalValues.Length; i++)
        {
            if (conditionalValues[i] <= currentKillCount)
                return additiveValues[i];
        }
        return 0f;
    }
}
