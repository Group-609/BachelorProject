using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class KillCountCondition : ICondition
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static KillCountCondition instance = null;
    private static readonly object padlock = new object();

    private KillCountCondition() { }

    public static KillCountCondition Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new KillCountCondition();
                }
                return instance;
            }
        }
    }

    // IMPORTANT! Both arrays have to be the same length
    private static readonly int[] dpgValues = new int[] { 0, 5, 10, 15, 20 }; // kill count numbers, by which the DDAA's additive values would be mapped
    private static readonly float[] dpgAdditiveValues = new float[] { -0.2f, 0.2f, 0.4f, 0.6f, 1f }; // additive values to multiplier

    private int currentKillCount;

    public float ConditionValue
    {
        get => currentKillCount;
        set
        {
            // should start the configuration of every DDAA here
            currentKillCount = (int) value;
            DDAEngine.Instance.AdjustDPG(GetAdditiveValue(dpgValues, dpgAdditiveValues));
        }
    }

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
