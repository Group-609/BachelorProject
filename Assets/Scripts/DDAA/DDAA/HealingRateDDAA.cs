﻿using System.Collections;
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
    private static readonly float baseHealingPoint = 1f;
    private static readonly int minHealingRate = 8;
    private static readonly float dpgContribution = 0.1f;
    private static readonly float healingPointContribution = 2f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] levelProgression = new float[] { 0.5f, 0.75f, 0f, 1.25f, 1.5f }; // how many times were they faster than needed

    private static readonly float[] levelProgressionPointAdditiveValues = new float[] { 2f, 1f, 0f, -1f, -2f }; // additive values to point directly
    private static readonly float[] levelProgressionMultiplierAdditiveValues = new float[] { 1f, 0.5f, 0f, -1.5f, -2f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, they will change during the gameplay
    private float healingMultiplier = 1f;
    private float healingPoint = baseHealingPoint;

    // THE IN-GAME VALUE USED
    public int healingRate = (int) DDAEngine.Instance.CalculateInGameValue(baseHealingPoint, healingPointContribution, dpgContribution, minHealingRate);

    // This listener is important if some action has to take place, when the IN-GAME VALUE is changed.
    // Otherwise the variable, which uses it (e.g. PlayerController), will not be notified.
    private IValueChangeListener healingListener;

    public void SetHealingListener(IValueChangeListener listener)
    {
        healingListener = listener;
    }
    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        healingMultiplier += DDAEngine.Instance.GetAdditiveValue(
                LevelProgressionCondition.Instance.ConditionValue,
                levelProgression,
                levelProgressionMultiplierAdditiveValues
            );
        // adjust multiplier and point values
        healingPoint = baseHealingPoint * healingMultiplier; // possible to add value directly

        //set healing rate
        healingRate = (int)DDAEngine.Instance.CalculateInGameValue(healingPoint, healingPointContribution, dpgContribution, minHealingRate + addToInGameValue);

        if (healingListener != null)
        {
            healingListener.OnValueChanged(healingRate);
        }
    }
}
