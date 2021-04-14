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

    public int currentLevel;
    private static readonly int[] expectedFinishTimes = new int[] { 50, 100, 180 }; // adjust these. Expected time (in sec), when player should complete level

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    private static readonly float[] dpgValues = new float[] { 0.8f, 0f, 1.2f}; // finish level time comparison, by which the DDAA's additive values would be mapped
    private static readonly float[] dpgAdditiveValues = new float[] { 2f, 0f, -2.5f }; // additive values to DPG point

    //holds the value of player's speed compared to what is expected (time/expectedTime)
    private float currentConditionalValue;

    private float time;

    public bool isGameFinished = false;

    public float ConditionValue
    {
        get => currentConditionalValue;
        set
        {
            // should start the configuration of every DDAA here
            currentConditionalValue = value;
            DDAEngine.AdjustDPG(currentConditionalValue, dpgValues, dpgAdditiveValues);
        }
    }

    //call this in Update
    public void AddDeltaTime(float deltaTime)
    {
        time += deltaTime;
    }

    public void LevelFinished()
    {
        if (currentLevel < expectedFinishTimes.Length)
        {
            Debug.Log("Time spent for level " + currentLevel + ": " + time + ". Expected time was: " + expectedFinishTimes[currentLevel]);
            if (DDAEngine.isDynamicAdjustmentEnabled)
                ConditionValue = time / expectedFinishTimes[currentLevel];
            currentLevel++;
            //Debug.Log("Adjusted conditional value. Started level: " + currentLevel);

            levelProgressionListeners.ForEach(listener => listener.OnLevelFinished());
        } 
        else
        {
            Debug.Log("Game is finished");
            isGameFinished = true;
        }
    }


    //listener implementation, to notify different parts of the game that the level has finished
    private List<LevelProgressionListener> levelProgressionListeners = new List<LevelProgressionListener>();

    public interface LevelProgressionListener
    {
        void OnLevelFinished();
    }

    public void AddLevelProgressionListener(LevelProgressionListener listener)
    {
        levelProgressionListeners.Add(listener);
    }

}
