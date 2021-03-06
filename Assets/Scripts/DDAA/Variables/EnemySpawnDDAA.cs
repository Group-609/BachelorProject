﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySpawnDDAA : IDDAA
{
    // --------------------------------- //
    // Singleton related implementation //
    //----------------------------------//

    private static EnemySpawnDDAA instance = null;
    private static readonly object padlock = new object();

    private EnemySpawnDDAA() { }

    public static EnemySpawnDDAA Instance
    {
        get
        {
            // Locks this part of the code, so singleton is not instantiated twice at the same time 
            // on two different threads (thread-safe method)
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new EnemySpawnDDAA();
                }
                return instance;
            }
        }
    }

    // NOTE!!! 
    // All of the parameters below are the ones to change, when adjusting the DDA (unless there's a bug)

    //static parameters
    private static readonly float baseSpawnPoint = 5f;
    public static readonly int minSpawnAmount = 4;
    // DPG is individual, so it shouldn't change team based DDA variables (based on one player's individual skill, spawn amount shouldn't change
    private static readonly float dpgContribution = 0f; 
    private static readonly float spawnPointContribution = 1f;

    // IMPORTANT! Both arrays have to be the same length
    private static readonly float[] levelProgressionPointAdditiveValues = new float[] { 2f, 1.5f, 0f, -1.5f, -2f }; // additive values to point directly
    private static readonly float[] levelProgressionMultiplierAdditiveValues = new float[] { 0.8f, 0.5f, 0f, -0.5f, -0.8f }; // additive values to multiplier

    // Mutable parameters. 
    // Do not ajust these, it will change during the gameplay
    public float spawnFloatingPoint = baseSpawnPoint;
    public float spawnMultiplier = 1f;
    private float spawnPoint = baseSpawnPoint;

    // THE IN-GAME VALUE USED
    // I gave it initial value as minimum reload time, though it could be any we wish (probably somewhere in the middle)
    public int spawnAmount = (int) DDAEngine.CalculateInGameValue(baseSpawnPoint, spawnPointContribution, dpgContribution, minSpawnAmount);

    // This listener is important if some action has to take place, when the reload time is changed.
    // Otherwise the variable, which holds reload time, will not be notified.
    private IValueChangeListener spawnListener;

    public void SetSpawnListener(IValueChangeListener listener)
    {
        spawnListener = listener;
    } 

    public void AdjustInGameValue(int addToInGameValue = 0)
    {
        spawnFloatingPoint = Mathf.Max(
            0f,
            spawnFloatingPoint + DDAEngine.GetAdditiveValue(
                LevelProgressionCondition.Instance.ConditionValue,
                LevelProgressionCondition.levelProgression,
                levelProgressionPointAdditiveValues
            )
        );

        spawnMultiplier = Mathf.Max(
            0f,
            spawnMultiplier + DDAEngine.GetAdditiveValue(
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
        spawnPoint = spawnFloatingPoint * spawnMultiplier; // possible to add value directly

        //set enemy spawn amount
        spawnAmount = (int) DDAEngine.CalculateInGameValue(spawnPoint, spawnPointContribution, dpgContribution, minSpawnAmount + addToInGameValue);

        if (spawnListener != null)
        {
            spawnListener.OnValueChanged(spawnAmount);
        }
    }

    public void Reset()
    {
        spawnMultiplier = 1f;
        spawnFloatingPoint = baseSpawnPoint;
        spawnPoint = baseSpawnPoint;
        CalculateInGameValue();
    }
}
