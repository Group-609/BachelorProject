using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealingRateDDAA : IDDAA
{

    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static HealingRateDDAA instance = null;
    private static readonly object padlock = new object();

    private HealingRateDDAA() { }

    public static HealingRateDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new HealingRateDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float baseHealingPoint = 3f;
    public static readonly float minHealingRate = 4f;
    private static readonly float dpgContribution = 0.1f;
    private static readonly float healingPointContribution = 1f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] levelProgressionPointAdditiveValues = new float[] { -2f, -1f, 0f, 1f, 2f }; // additive values to point directly
    private static readonly float[] levelProgressionMultiplierAdditiveValues = new float[] { -0.5f, -0.2f, 0f, 0.2f, 0.5f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, they will change during the gameplay
    public float healingMultiplier = 1f;
    private float healingPoint = baseHealingPoint;

    // THE IN-GAME VALUE USED
    public float healingRate = DDAEngine.CalculateInGameValue(baseHealingPoint, healingPointContribution, dpgContribution, minHealingRate);

    // This listener is important if some action has to take place, when the IN-GAME VALUE is changed.
    // Otherwise the variable, which uses it (e.g. PlayerController), will not be notified.
    private IValueChangeListener healingListener;

    public void SetHealingListener(IValueChangeListener listener)
    {
        healingListener = listener;
    }
    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        healingMultiplier = Mathf.Max(
            0f,
            healingMultiplier + DDAEngine.GetAdditiveValue(
                LevelProgressionCondition.Instance.ConditionValue,
                LevelProgressionCondition.levelProgression,
                levelProgressionMultiplierAdditiveValues
            )
        );

        CalculateInGameValue(addToInGameValue);
    }

    private void CalculateInGameValue(int addToInGameValue = 0)
    {
        // adjust multiplier and point values
        healingPoint = baseHealingPoint * healingMultiplier; // possible to add value directly

        //set healing rate
        healingRate = DDAEngine.CalculateInGameValue(healingPoint, healingPointContribution, dpgContribution, minHealingRate + addToInGameValue);

        if (healingListener != null)
        {
            healingListener.OnValueChanged(healingRate);
        }
    }

    public void Reset()
    {
        healingMultiplier = 1f;
        CalculateInGameValue();
    }
}
