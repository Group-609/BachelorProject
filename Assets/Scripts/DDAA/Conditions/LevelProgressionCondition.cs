using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelProgressionCondition : ICondition
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static LevelProgressionCondition instance = null;
    private static readonly object padlock = new object();

    private LevelProgressionCondition() { }

    public static LevelProgressionCondition Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new LevelProgressionCondition();
                }
                return instance;
            }
        }
    }

    private int currentLevel;
    private static readonly int[] expectedFinishTimes = new int[] { 10, 20, 30 }; // adjust these

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    private static readonly int[] dpgValues = new int[] { 0, 5, 10, 15, 20 }; // kill count numbers, by which the DDAA's additive values would be mapped
    private static readonly float[] dpgAdditiveValues = new float[] { -1f, 0f, 0.5f, 1f, 1.5f }; // additive values to DPG point

    //holds the value of player's speed compared to what is expected (time/expectedTime)
    private float currentConditionalValue;

    private float time;

    public float ConditionValue
    {
        get => currentConditionalValue;
        set
        {
            // should start the configuration of every DDAA here
            currentConditionalValue = value;
            DDAEngine.Instance.AdjustDPG(currentConditionalValue, dpgValues, dpgAdditiveValues);
        }
    }

    //call this in Update
    public void AddDeltaTime(float deltaTime)
    {
        time += deltaTime;
    }

    public void LevelFinished()
    {
        try
        {
            ConditionValue = time / expectedFinishTimes[currentLevel];
            currentLevel++;
        } catch(NullReferenceException e)
        {
            Debug.Log("Game has finished");
        }
    }
}
