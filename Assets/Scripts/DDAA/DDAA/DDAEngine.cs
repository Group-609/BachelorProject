using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This class takes care of the calculating the in-game value by using formula.

    Also, it holds and takes care of Difficulty Point Global variable
*/

public sealed class DDAEngine
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static DDAEngine instance = null;
    private static readonly object padlock = new object();

    private DDAEngine(){}

    public static DDAEngine Instance { 
        get 
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new DDAEngine();
                }
                return instance;
            }
        } 
    }

    // Used functions 
    // Call them using DDAEngine.Instance.FunctionName

    private float difficultiesPointGlobal = 10;

    public void AdjustDPG(float conditionValue, int[] conditionalValues, float[] additiveValues)
    {
        difficultiesPointGlobal += GetAdditiveValue(conditionValue, conditionalValues, additiveValues);
    }

    // Based on ConditionalValue this function should return what adjustment should be done to DDAA's multiplier/point
    public float GetAdditiveValue(float conditionValue, int[] conditionalValues, float[] additiveValues)
    {
        for (int i = 0; i < conditionalValues.Length; i++)
        {
            if (conditionalValues[i] >= conditionValue)
                return additiveValues[i];
        }
        return additiveValues[additiveValues.Length - 1];
    }

    // Based on ConditionalValue this function should return what adjustment should be done to DDAA's multiplier/point
    public float GetAdditiveValue(float conditionValue, float[] conditionalValues, float[] additiveValues)
    {
        for (int i = 0; i < conditionalValues.Length; i++)
        {
            if (conditionalValues[i] >= conditionValue)
                return additiveValues[i];
        }
        return additiveValues[additiveValues.Length - 1];
    }

    public float CalculateInGameValue(float point, float pointContribution, float dpgContribution, float minValue = 0f) 
    {
        return minValue + (point * pointContribution) + (difficultiesPointGlobal * dpgContribution);
    }
}
