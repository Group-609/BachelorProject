using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void AdjustDPG(float additiveValue)
    {
        difficultiesPointGlobal += additiveValue;
    }

    public float CalculateInGameValue(float point, float dpgContribution, float minValue = 0f) 
    {
        return minValue + point + difficultiesPointGlobal * dpgContribution;
    }
}
