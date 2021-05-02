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
    private static readonly int[] expectedTimeSpendInCombat = new int[] { 50, 120, 200 }; // adjust these (expected time in seconds)

    // IMPORTANT! Both arrays have to be the same length
    // They both are optional to have (based on our decisions what condition affects what variables etc.)
    public static readonly float[] levelProgression = new float[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f }; // how many times were they faster than needed
    private static readonly float[] dpgAdditiveValues = new float[] { 1f, 0.5f, 0f, -0.5f, -1f }; // additive values to DPG point

    //holds the value of player's speed compared to what is expected (time/expectedTime)
    private float currentConditionalValue = 1f;

    private float time;

    public bool isGameFinished = false;

    public float ConditionValue
    {
        get => currentConditionalValue;
        set
        {
            // should start the configuration of every DDAA here
            currentConditionalValue = value;
            DDAEngine.AdjustDPG(currentConditionalValue, levelProgression, dpgAdditiveValues);
        }
    }

    public void Reset()
    {
        isGameFinished = false;
        currentConditionalValue = 1f;
        time = 0f;
        currentLevel = 0;
    }

    //call this in Update
    public void AddDeltaTime(float deltaTime)
    {
        time += deltaTime;
    }

    public void LevelFinished()
    {
        if (currentLevel < expectedTimeSpendInCombat.Length)
        {
            Debug.Log("Time spent for level " + currentLevel + ": " + time + ". Expected time was: " + expectedTimeSpendInCombat[currentLevel]);
            ConditionValue = time / expectedTimeSpendInCombat[currentLevel];
            currentLevel++;
            //Debug.Log("Adjusted conditional value. Started level: " + currentLevel);

            levelProgressionListeners.ForEach(listener => listener.OnLevelFinished());
            if(currentLevel == expectedTimeSpendInCombat.Length)
            {
                Debug.Log("Game is finished");
                isGameFinished = true;
            }
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
